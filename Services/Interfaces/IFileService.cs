using FileSharing.Models;

namespace FileSharing.Services.Interfaces;

public interface IFileService {
    Task<IEnumerable<FileItemGet>> GetAllFilesAsync(string userId);
    
    Task<FileItemGet> CreateFolderAsync(FolderCreate dto, string userId);
    Task<FileItemGet> SoftDeleteFileAsync(string fileId, string userId);
    Task<FileItemGet> RestoreFileAsync(string fileId, string userId);
    Task<FileItemGet> ToggleStarredAsync(string fileId, string userId);
    Task<FileItemGet> RenameAsync(string fileId, string userId, FileRename dto);

    Task<bool> ShareFileAsync(string fileId,string userId,string permissionType);
    
}