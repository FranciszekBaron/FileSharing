using FileSharing.Models;

public interface IFileItemRepository : IRepositoryBase<FileItem>
{
    Task<IEnumerable<FileItem>> GetFilesByOwnerAsync(string ownerId);
    Task<IEnumerable<FileItem>> GetFilesInFolderAsync(string parentId);
    Task<IEnumerable<FileItem>> GetStarredFilesAsync();

    Task<FileItem> GetByIdAsync(string id);

    Task<List<UserGetDto>> GetAllSharedUsersAsync(string userId);

    Task<UserGetDto> GetOwnerAsync(string fileId);



}