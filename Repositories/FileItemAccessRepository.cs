
using Microsoft.EntityFrameworkCore;

public class FileItemAccessRepository : RepositoryBase<FileItemAccess>, IFileItemAccessRepository
{

    private readonly FileSharingDbContext _dbContext;
    public FileItemAccessRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<FileItemAccess>> GetSharedFiles(string userId)
    {
        return await _dbContext.FilesAccesess
        .Where(a => a.UserId == userId)
        .Include(a => a.FileItem)         // ✅ Załaduj FileItem
            .ThenInclude(f => f.Owner)    // ✅ Załaduj Owner
        .ToListAsync();
    }
}