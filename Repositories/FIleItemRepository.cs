using System.Linq.Expressions;
using FileSharing.Models;
using Microsoft.EntityFrameworkCore;

public class FileItemRepository : RepositoryBase<FileItem>, IFileItemRepository
{

    private readonly FileSharingDbContext _dbContext;
    public FileItemRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
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

    public async Task<FileItem?> GetByIdAsync(string id)
    {
       return _dbContext.FileItems.FirstOrDefault(e=>e.Id == id);
    }

    public async Task<List<UserGetDto>> GetAllSharedUsersAsync(string userId)
    {
        var ownerFileIds = await _dbContext.FileItems
        .Where(f => f.OwnerId == userId) // Pliki gdzie OwnerId == "my-user-id"
        .Select(f => f.Id)
        .ToListAsync();

        var sharedUsers = await _dbContext.FilesAccesess
        .Where(fa => ownerFileIds.Contains(fa.FileItemId)) // id zawieraje sie w ownedFiles 
        .Include(fa => fa.User) // Załaduj User do tego 
        .Select(fa => new UserGetDto
        {
            UserName = fa.User.UserName,
            Email = fa.User.Email,
            Avatar = fa.User.Avatar,
        })
        .Distinct()
        .ToListAsync();

        return sharedUsers;
    }

    public async Task<UserGetDto?> GetOwnerAsync(string fileId)
    {
        var owner = await _dbContext.FileItems
        .Where(f=>f.Id == fileId)
        .Include(f => f.Owner)
        .Select(f=> new UserGetDto
        {
                UserName = f.Owner.UserName,
                Email = f.Owner.Email,
                Avatar = f.Owner.Avatar,


        }).FirstOrDefaultAsync();

        return owner;
    }
}