using System.Linq.Expressions;
using FileSharing.Models;

public class FileItemRepository : RepositoryBase<FileItem>,IFileItemService
{
    public FileItemService(FileSharingDbContext dbContext) : base(dbContext)
    {
    }

    public void Create(FileItem entity)
    {
        Create(entity);
    }

    public void Delete(FileItem entity)
    {
        Delete(entity);
    }

    public Task<IEnumerable<FileItem>> GetAllItems()
    {
        return GetAllAsync();
    }

    public Task<IEnumerable<FileItem>> GetByIdAsync(string id)
    {
       return GetByConditionAsync(e=>e.Id == id);
    }

    public void Update(FileItem entity)
    {
        Update(entity);
    }
}