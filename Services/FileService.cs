


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

    public async Task<FileItemGet> GetFileById(string fileId, string userId)
    {
        var fileToGet = await  _repository.fileItemRepo.GetByIdAsync(fileId);

        if (fileToGet == null)
            throw new KeyNotFoundException("File not found");

        bool isOwned = fileToGet.OwnerId == userId;
        var sharedFiles = await _repository.fileItemAccessRepo.GetSharedFiles(userId);
        bool isShared = sharedFiles.Any(f=>f.FileItemId == fileId && f.UserId == userId);

        if(!isOwned && !isShared)
            throw new UnauthorizedAccessException();

        return new FileItemGet
        {
            Id = fileToGet.Id,
            Name = fileToGet.Name,
            Type = fileToGet.Type,
            OwnerId = fileToGet.OwnerId,
            OwnerName =  "",
            ParentId = fileToGet.ParentId,
            ModifiedDate = fileToGet.ModifiedDate,
            Permission = "owner",
            Starred = fileToGet.Starred ?? false,
            Deleted = fileToGet.Deleted ?? false,
        };
    }

    public async Task<FileItemGet> CreateFolderAsync(FolderCreate dto, string userId)
    {
        //Czy nazwa jest poprawna
        ValidateName(dto.Name);

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
        ValidateName(dto.Name);
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


    public async Task<bool> ShareFileAsync(string fileId,string userId,FileItemAccessCreate dto)
    {   

        //1. Czy jest taki rekord w bazie? 
        //======================================================
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var itemToShare = allItems.Where(i => i.Id == fileId).FirstOrDefault();

        if (itemToShare == null)
            throw new ArgumentException($"There is no item with id {fileId} to share, either you don't own it");
        //======================================================

        
        foreach (var email in dto.Emails){

            // 2. Znajdź użytkownika po email/username z dto
            //======================================================
            var targetUser = await _repository.userRepo.GetByEmailAsync(email); // lub GetByUsername

            
            
            if(targetUser == null)
                throw new ArgumentException($"User {email} not found.");
            //======================================================    


            //2. Czy nie udostępniasz sobie? 
            //======================================================
            if(itemToShare.OwnerId == targetUser.Id || userId == targetUser.Id)
                throw new ArgumentException($"You cannot share a file to yourself!");
            //======================================================

            //3. Czy juz nie udostępniony tej osobie?
            //======================================================
            
            var alreadyShared = await _repository.fileItemAccessRepo.IsAlreadyShared(fileId, targetUser.Id);
            if(alreadyShared)
                throw new InvalidOperationException($"You cannot share a file twice!");
            //======================================================

            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(targetUser.Id + ", " + dto.Permission);
            Console.ResetColor();   

            //4. Czy dostęp jest editor/viewer 
            //======================================================
            if(dto.Permission.Trim() != "editor" && dto.Permission.Trim() != "viewer")
                throw new ArgumentException($"There is no such permission type: {dto.Permission}");
            //======================================================



            var fileItemAccess = new FileItemAccess
            {
                UserId = targetUser.Id,
                FileItemId = fileId,
                PermissionType = dto.Permission,
                SharedDate = DateTime.UtcNow
            };

            _repository.fileItemAccessRepo.Create(fileItemAccess);
            await _repository.SaveAsync();
        }

        return true;
    }

    public async Task<List<UserGetDto>> GetAllSharedUsers(string userId)
    {
        if(userId == null)
            throw new KeyNotFoundException($"User with id {userId} doesn't exist");
        
        // Ludzie którym JA udostępniłem
        var usersISharedWith = await _repository.fileItemRepo.GetAllSharedUsersAsync(userId);
        
        // Ludzie którzy MNIEudostępnili
        var usersWhoSharedWithMe = await _repository.fileItemAccessRepo.GetUsersWhoSharedWithMe(userId);
        
        // Połącz obie listy i usuń duplikaty
        var allUsers = usersISharedWith
            .Union(usersWhoSharedWithMe)
            .DistinctBy(u => u.Email)
            .ToList();
        
        return allUsers;
    }


    public async Task<FileItemGet> UploadFileAsync(string userId,FileUploadDto dto)
    {

        // Czy jest plik w requescie
        //======================================================
        if (dto.File == null || dto.File.Length == 0)
            throw new ArgumentException("File is required");
        
        // Czy plik nie jest za duzy do przechowywania
        const long MAX_FILE_SIZE = 100 * 1024 * 1024;
        if (dto.File.Length > MAX_FILE_SIZE)
            throw new ArgumentException($"File is too large (max 100MB)");
        //======================================================
        
        //Czy jest taki rekord w bazie? 
        //======================================================
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var exists = allItems.Any(f => f.Name == dto.File.FileName && f.ParentId == dto.ParentId && f.DeletedAt == null);

        if (exists)
           throw new InvalidOperationException($"File '{dto.File.FileName}' already exists");
        //======================================================

        //Czy nazwa jest poprawna?
        //======================================================
        ValidateName(dto.File.FileName);
        //======================================================


        ValidateMimeType(Path.GetExtension(dto.File.FileName),dto.File.ContentType);

        var path = await SaveFileToStorageAsync(dto.File, userId);


        //Czy signature poprawny?
        //======================================================
        ValidateFileSignature(dto.File, Path.GetExtension(dto.File.FileName));
        //======================================================


        var fileItem = new FileItem
        {
            Name = dto.File.FileName,
            Type = Path.GetExtension(dto.File.FileName).TrimStart('.'),
            Size = dto.File.Length,
            ModifiedDate = DateTime.UtcNow,
            OwnerId = userId,
            ParentId = dto.ParentId,
            ContentUrl = path,
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
            Starred = fileItem.Starred ?? false,
            Deleted = fileItem.Deleted ?? false,
        };
    }

   
    private void ValidateMimeType(string extension, string contentType)
    {
        var allowedMimeTypes = new Dictionary<string, string[]>
        {
            { ".txt", new[] { "text/plain" } },
            { ".pdf", new[] { "application/pdf" } },
            { ".doc", new[] { "application/msword" } },
            { ".docx", new[] { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } }
        };
        
        if (!allowedMimeTypes.ContainsKey(extension))
            throw new ArgumentException($"Unsupported extension: {extension}");
        
        if (!allowedMimeTypes[extension].Contains(contentType))
            throw new ArgumentException($"Invalid MIME type for {extension}. Got: {contentType}");
    }

    private void ValidateName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("Name is required");
        
        if (fileName.Length > 255)
            throw new ArgumentException("Name is too long (max 255 characters)");

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException("Name contains invalid characters");

        string[] reservedNames = { ".", "..", "CON", "PRN", "AUX", "NUL", "COM1", "LPT1" };
        if (reservedNames.Contains(fileName.ToUpper()))
            throw new ArgumentException("Name is reserved");

        if (fileName != fileName.Trim())
            throw new ArgumentException("Name cannot start or end with whitespace");
    }

    private async Task<bool> ValidateFileSignature(IFormFile file, string expectedType)
    {
        var signatures = new Dictionary<string, byte[][]>
        {
            { "pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },  // %PDF
            { "txt", new[] { new byte[] { } } },  // Tekst nie ma magic bytes
            { "doc", new[] { 
                new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }  // DOC (OLE)
            }},
            { "docx", new[] { 
                new byte[] { 0x50, 0x4B, 0x03, 0x04 }  // DOCX (ZIP)
            }}
        };
        
        if (!signatures.ContainsKey(expectedType))
            return false;  // Brak walidacji dla tego typu
        
        using var reader = new BinaryReader(file.OpenReadStream());
        var headerBytes = reader.ReadBytes(8);  // Czytaj pierwsze 8 bajtów
        
        foreach (var signature in signatures[expectedType])
        {
            if (headerBytes.Take(signature.Length).SequenceEqual(signature))
                return true;  
        }
        
        return false;  
    }


    public async Task<string> SaveFileToStorageAsync(IFormFile file,string userId)
    {

        //TODO - zmienić na _configuration
        string uploadsFolder = Path.Combine("/Users/franekbaron/Desktop/Projekty/FileSharingServer/uploads",userId);
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        string filePath = Path.Combine(uploadsFolder,uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }

    public async Task<(FileStream fileStream, string fileName, string contentType)> DownloadFileAsync(string fileId, string userId)
    {
        var fileToDownload = await  _repository.fileItemRepo.GetByIdAsync(fileId);

        if (fileToDownload == null)
            throw new KeyNotFoundException("File not found");

        bool isOwned = fileToDownload.OwnerId == userId;
        var sharedFiles = await _repository.fileItemAccessRepo.GetSharedFiles(userId);
        bool isShared = sharedFiles.Any(f=>f.FileItemId == fileId && f.UserId == userId);

        if(!isOwned && !isShared)
            throw new UnauthorizedAccessException();

        if (fileToDownload.DeletedAt != null)
            throw new InvalidOperationException("Cannot download deleted file");
        
        if (fileToDownload.Type == "folder")
            throw new InvalidOperationException("Cannot download folders");

        if(fileToDownload.ContentUrl == null)
            throw new InvalidOperationException($"Content of file with id {fileId}' doesn't exists");



        var stream = new FileStream(fileToDownload.ContentUrl,FileMode.Open);

        

        return (stream,fileToDownload.Name,GetContentType(fileToDownload.Type));
    }

    private string GetContentType(string fileType)
    {
        return fileType switch
        {
            "txt" => "text/plain",
            "pdf" => "application/pdf",
            "doc" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"  // Domyślny dla nieznanych typów
        };
    }

    public async Task<bool> PermanentFileDeleteAsync(string fileId, string userId)
    {
        var allItems = await  _repository.fileItemRepo.GetFilesByOwnerAsync(userId);  
        var itemToDelete = allItems.Where(i => i.Id == fileId).FirstOrDefault();

        if (itemToDelete == null)
            throw new KeyNotFoundException("File not found");
        
        if (itemToDelete.OwnerId != userId)
            throw new UnauthorizedAccessException("Only owner can delete file");
    
        // 3. Sprawdź czy już usunięty
        if (itemToDelete.Deleted != true)
            throw new InvalidOperationException("File is not set to be deleted");
        
        _repository.fileItemRepo.Delete(itemToDelete);
        await _repository.SaveAsync();

        return true;
    }

    public async Task<List<UserGetDto>> GetUsersWithAccess(string fileId, string userId)
    {
        var owner = await _repository.fileItemRepo.GetOwnerAsync(fileId);

        var usersWithAccess = await _repository.fileItemAccessRepo.GetAllUserAccessessForFileAsync(fileId);


        var allUsers = usersWithAccess
            .Union(new List<UserGetDto> {owner})
            .DistinctBy(u=>u.Email)
            .ToList();

        return allUsers;
    }
}

