


using FileSharing.Models;
using FileSharing.Services.Interfaces;

namespace FileSharing.Services;

public class FileService : IFileService {
    private readonly IRepositoryWrapper _repository;

    public FileService(IRepositoryWrapper repository) {
        _repository = repository;
    }

    
    public async Task<IEnumerable<FileItemGet>> GetAllFilesAsync(string userId)
    {
        var ownedItems = await _repository.fileItemRepo.GetFilesByOwnerAsync(userId);

        var sharedItems = await _repository.fileItemAccessRepo.GetSharedFiles(userId);

        var sharedFiles = sharedItems.Select(a => a.FileItem);

        var allFiles = ownedItems.Union(sharedFiles);
        

        //Wez udostepnione Tobie pliki z 
        return allFiles.Select(f=> new FileItemGet
        {
            Id = f.Id,
            Name = f.Name,
            Type = f.Type,
            Size = f.Size,
            ModifiedDate = f.ModifiedDate,
            OwnerId = f.OwnerId,
            OwnerName = f.Owner?.UserName ?? "",
            ParentId = f.ParentId,
            Starred = f.Starred ?? false,
            Deleted = f.Deleted ?? false,
            Permission = "owner"
        });
    }

    public async Task<FileItemGet> CreateFolderAsync(FolderCreate dto, string userId)
    {
        //Czy nazwa jest poprawna
        //======================================================
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentException("Name is required");
        
        if (dto.Name.Length > 255)
            throw new ArgumentException("Name is too long (max 255 characters)");

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (dto.Name.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException("Name contains invalid characters");

        string[] reservedNames = { ".", "..", "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };
        if (reservedNames.Contains(dto.Name.ToUpper()))
            throw new ArgumentException("Name is reserved");

        if (dto.Name != dto.Name.Trim())
        throw new ArgumentException("Name cannot start or end with whitespace");
        //======================================================


        //Czy ParentId istnieje w ogóle? 
        //======================================================
        if (!string.IsNullOrEmpty(dto.ParentId))
        {
            var allFiles = await _repository.fileItemRepo.GetFilesByOwnerAsync(userId);
            var parentFolder = allFiles.FirstOrDefault(f => f.Id == dto.ParentId);
            
            if (parentFolder == null)
                throw new KeyNotFoundException("Parent folder not found");
            
            if (parentFolder.Type != "folder")
                throw new InvalidOperationException("Parent must be a folder");
            
            if (parentFolder.DeletedAt != null)
                throw new InvalidOperationException("Cannot add to deleted folder");
        }
        //=====================================================



        //Czy jest taki rekord w bazie
        //======================================================
        var allFilesForDuplicate = await _repository.fileItemRepo.GetAllAsync();

        var exists = allFilesForDuplicate.Any(f => 
            f.Name == dto.Name.Trim() && 
            f.ParentId == dto.ParentId && 
            f.Type == "folder" &&
            f.DeletedAt == null
        );

        if (exists)
            throw new InvalidOperationException($"File '{dto.Name}' already exists");
        //======================================================


        var fileItem = new FileItem
        {
            Name = dto.Name,
            Type = "folder",
            ModifiedDate = DateTime.UtcNow,
            OwnerId = userId,
            ParentId = dto.ParentId
        };

        _repository.fileItemRepo.Create(fileItem);
        await _repository.SaveAsync();

        return new FileItemGet
        {
            Id = fileItem.Id,
            Name = fileItem.Name,
            Type = fileItem.Type,
            OwnerId = fileItem.OwnerId,
            OwnerName =  "",
            ParentId = fileItem.ParentId,
            ModifiedDate = fileItem.ModifiedDate,
            Permission = "owner",
            Starred = false,
            Deleted = false
        };

    }


    public async Task<FileItemGet> SoftDeleteFileAsync(string fileId, string userId)
    {
        var allItems = await _repository.fileItemRepo.GetFilesByOwnerAsync(userId);
        var itemToDelete = allItems.Where(x => x.Id == fileId).FirstOrDefault();

        if (itemToDelete == null)
            throw new ArgumentException($"There is no item with id {fileId}");

        if (itemToDelete.OwnerId != userId)
            throw new UnauthorizedAccessException("Only owner can delete file");
    
        // 3. Sprawdź czy już usunięty
        if (itemToDelete.Deleted == true)
            throw new InvalidOperationException("File already deleted");
        
        itemToDelete.Deleted = true;
        itemToDelete.DeletedAt = DateTime.UtcNow;

        _repository.fileItemRepo.Update(itemToDelete);
        await _repository.SaveAsync();

        return new FileItemGet
        {
            Id = itemToDelete.Id,
            Name = itemToDelete.Name,
            Type = itemToDelete.Type,
            OwnerId = itemToDelete.OwnerId,
            OwnerName =  "",
            ParentId = itemToDelete.ParentId,
            ModifiedDate = itemToDelete.ModifiedDate,
            Permission = "owner",
            Starred = itemToDelete.Starred ?? false,
            Deleted = itemToDelete.Deleted ?? false,
        };
    }

    public async Task<FileItemGet> RestoreFileAsync(string fileId, string userId)
    {
        var allItems = await _repository.fileItemRepo.GetFilesByOwnerAsync(userId);
        var itemToRestore = allItems.Where(x => x.Id == fileId).FirstOrDefault();

        if (itemToRestore == null)
            throw new ArgumentException($"There is no item with id {fileId}");

        if (itemToRestore.OwnerId != userId)
            throw new UnauthorizedAccessException("Only owner can delete file");
    
        // 3. Sprawdź czy już usunięty
        if (itemToRestore.Deleted == false)
            throw new InvalidOperationException("File is not deleted");
        
        itemToRestore.Deleted = false;
        itemToRestore.DeletedAt = null;
        itemToRestore.ModifiedDate = DateTime.UtcNow;

        _repository.fileItemRepo.Update(itemToRestore);
        await _repository.SaveAsync();

        return new FileItemGet
        {
            Id = itemToRestore.Id,
            Name = itemToRestore.Name,
            Type = itemToRestore.Type,
            OwnerId = itemToRestore.OwnerId,
            OwnerName =  "",
            ParentId = itemToRestore.ParentId,
            ModifiedDate = itemToRestore.ModifiedDate,
            Permission = "owner",
            Starred = itemToRestore.Starred ?? false,
            Deleted = itemToRestore.Deleted ?? false,
        };
    }

    public async Task<FileItemGet> ToggleStarredAsync(string fileId, string userId)
    {
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var itemToStarred = allItems.Where(i => i.Id == fileId).FirstOrDefault();

        if (itemToStarred == null)
            throw new ArgumentException($"There is no item with id {fileId}");


        bool starredState = !(itemToStarred.Starred ?? false);
        itemToStarred.Starred = starredState;
        _repository.fileItemRepo.Update(itemToStarred);
        await _repository.SaveAsync();

        return new FileItemGet
        {
            Id = itemToStarred.Id,
            Name = itemToStarred.Name,
            Type = itemToStarred.Type,
            OwnerId = itemToStarred.OwnerId,
            OwnerName =  "",
            ParentId = itemToStarred.ParentId,
            ModifiedDate = itemToStarred.ModifiedDate,
            Permission = "owner",
            Starred = starredState,
            Deleted = itemToStarred.Deleted ?? false
        };
    }

    public async Task<FileItemGet> RenameAsync(string fileId, string userId, FileRename dto)
    {
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var itemToRename = allItems.Where(i => i.Id == fileId).FirstOrDefault();

        if (itemToRename == null)
            throw new ArgumentException($"There is no item with id {fileId}");


        //Czy nazwa jest poprawna
        //======================================================
        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentException("Name is required");
        
        if (dto.Name.Length > 255)
            throw new ArgumentException("Name is too long (max 255 characters)");

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (dto.Name.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException("Name contains invalid characters");

        string[] reservedNames = { ".", "..", "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };
        if (reservedNames.Contains(dto.Name.ToUpper()))
            throw new ArgumentException("Name is reserved");

        if (dto.Name != dto.Name.Trim())
            throw new ArgumentException("Name cannot start or end with whitespace");
        //======================================================


        //Czy ma, czy dopisać rozszerzenie
        //======================================================
        if (itemToRename.Type != "folder")
        {
            string expectedExtension = $".{itemToRename.Type}";
            
            if (!dto.Name.EndsWith(expectedExtension, StringComparison.OrdinalIgnoreCase))
            {
                dto.Name += expectedExtension;
            }
        }

        itemToRename.Name = dto.Name;
        _repository.fileItemRepo.Update(itemToRename);
        await _repository.SaveAsync();

        return new FileItemGet
        {
            Id = itemToRename.Id,
            Name = dto.Name,
            Type = itemToRename.Type,
            OwnerId = itemToRename.OwnerId,
            OwnerName =  "",
            ParentId = itemToRename.ParentId,
            ModifiedDate = itemToRename.ModifiedDate,
            Permission = "owner",
            Starred = itemToRename.Starred ?? false,
            Deleted = itemToRename.Deleted ?? false,
        };

    }


    public async Task<bool> ShareFileAsync(string fileId,string userId,string permissionType)
    {   

        //Czy jest taki rekord w bazie? 
        //==================================================
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var itemToShare = allItems.Where(i => i.Id == fileId).FirstOrDefault();

        if (itemToShare == null)
            throw new ArgumentException($"There is no item with id {fileId} to share");
        //==================================================

        var fileItemAccess = new FileItemAccess
        {
            UserId = userId,
            FileItemId = fileId,
            PermissionType = permissionType,
            SharedDate = DateTime.UtcNow
        };

        _repository.fileItemAccessRepo.Create(fileItemAccess);
        await _repository.SaveAsync();

        return true;
    }
}

