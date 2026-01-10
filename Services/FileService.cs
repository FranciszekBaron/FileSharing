


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
        var allItems = await _repository.fileItemRepo.GetFilesByOwnerAsync(userId);
        return allItems.Select(f=> new FileItemGet
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

    public async Task<FileItemGet> CreateFileAsync(FileItemCreate dto, string userId)
    {

        if (string.IsNullOrEmpty(dto.Name))
            throw new ArgumentException("Name is required");

        
        var fileItem = new FileItem
        {
            Name = dto.Name,
            Type = dto.Type,
            ModifiedDate = dto.ModifiedDate,
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

    public Task<FileItem?> GetFileByIdAsync(string fileId, string userId)
    {
        throw new NotImplementedException();
    }

    
    public Task DeleteFileAsync(string fileId, string userId)
    {
        throw new NotImplementedException();
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
            Starred = false,
            Deleted = true
        };
    }
}

