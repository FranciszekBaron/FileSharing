using FileSharing.Models;

namespace FileSharing.Services.Interfaces;

public interface IFileService {
    Task<IEnumerable<FileItemGet>> GetAllFilesAsync(string userId);
    Task<FileItem?> GetFileByIdAsync(string fileId, string userId);
    Task<FileItemGet> CreateFileAsync(FileItemCreate dto, string userId);
    Task<FileItemGet> SoftDeleteFileAsync(string fileId, string userId);
    Task DeleteFileAsync(string fileId, string userId);
}