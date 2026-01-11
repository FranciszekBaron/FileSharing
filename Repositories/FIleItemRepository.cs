using System.Linq.Expressions;
using FileSharing.Models;

public class FileItemRepository : RepositoryBase<FileItem>, IFileItemRepository
{
    public FileItemRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
    }

    public Task<IEnumerable<FileItem>> GetAllItems()
    {
        return GetAllAsync();
    }

    public Task<IEnumerable<FileItem>> GetFilesByOwnerAsync(string ownerId){
        return GetByConditionAsync(e=>e.OwnerId == ownerId);
    }
    public Task<IEnumerable<FileItem>> GetFilesInFolderAsync(string parentId){
        return GetByConditionAsync(e=>e.ParentId == parentId);
    }
    public Task<IEnumerable<FileItem>> GetStarredFilesAsync(){
        return GetByConditionAsync(e=>e.Starred == true);
    }

    public Task<IEnumerable<FileItem>> GetByIdAsync(string id)
    {
       return GetByConditionAsync(e=>e.Id == id);
    }

}