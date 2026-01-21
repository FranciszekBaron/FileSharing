using FileSharing.Models;

namespace FileSharing.Services.Interfaces;

public interface IFileService {
    Task<IEnumerable<FileItemGet>> GetAllFilesAsync(string userId);
    
    Task<FileItemGet> CreateFolderAsync(FolderCreate dto, string userId);
    Task<FileItemGet> SoftDeleteFileAsync(string fileId, string userId);
    Task<bool> PermanentFileDeleteAsync(string fileId, string userId);
    Task<FileItemGet> RestoreFileAsync(string fileId, string userId);
    Task<FileItemGet> ToggleStarredAsync(string fileId, string userId);
    Task<FileItemGet> RenameAsync(string fileId, string userId, FileRename dto);

    Task<bool> ShareFileAsync(string fileId,string userId,FileItemAccessCreate dto);

    Task<FileItemGet> UploadFileAsync(string userId,FileUploadDto dto);

    Task<(FileStream fileStream,string fileName, string contentType)> DownloadFileAsync(string fileId,string userId);
    
    Task<FileItemGet> GetFileById(string fileId, string userId);

    Task<List<UserGetDto>> GetAllSharedUsers(string userId) ;

    Task<List<UserGetDto>> GetUsersWithAccess(string fileId,string userId);

    Task CleanUpOldFilesAsync();
}