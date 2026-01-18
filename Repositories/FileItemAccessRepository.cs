
using Microsoft.EntityFrameworkCore;

public class FileItemAccessRepository : RepositoryBase<FileItemAccess>, IFileItemAccessRepository
{

    private readonly FileSharingDbContext _dbContext;
    public FileItemAccessRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<UserGetDto>> GetAllUserAccessessForFileAsync(string fileId)
    {
        return await _dbContext.FilesAccesess
        .Where(f => f.FileItemId == fileId)
        .Include(fa => fa.User)
        .Select(fa => new UserGetDto
        {
            UserName = fa.User.UserName,
            Email = fa.User.Email,
            Avatar = fa.User.Avatar
        }).Distinct()
        .ToListAsync();

        
    }

    public async Task<IEnumerable<FileItemAccess>> GetSharedFiles(string userId)
    {
        return await _dbContext.FilesAccesess
        .Where(a => a.UserId == userId)
        .Include(a => a.FileItem)         // ✅ Załaduj FileItem
            .ThenInclude(f => f.Owner)    // ✅ Załaduj Owner
        .ToListAsync();
    }


    public async Task<List<UserGetDto>> GetUsersWhoSharedWithMe(string userId)
    {
        // Znajdź wszystkie pliki udostępnione Tobie
        var sharedWithMeOwners = await _dbContext.FilesAccesess
        .Where(fa => fa.UserId == userId) // Pliki udostępnione Tobie
        .Include(fa => fa.FileItem) // ✅ Załaduj plik
            .ThenInclude(f => f.Owner) // ✅ Załaduj właściciela pliku
        .Select(fa => new UserGetDto
        {
            UserName = fa.FileItem.Owner.UserName, // ✅ Owner, nie User!
            Email = fa.FileItem.Owner.Email,
            Avatar = fa.FileItem.Owner.Avatar,
        })
        .Distinct()
        .ToListAsync();
    
        return sharedWithMeOwners;
    }
    
    



    public async Task<bool> IsAlreadyShared(string fileId, string userId)
    {
        return await _dbContext.FilesAccesess
            .AnyAsync(f => f.FileItemId == fileId && f.UserId == userId);
    }
}