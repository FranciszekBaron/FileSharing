
public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly FileSharingDbContext _dbContext;

    private IFileItemRepository _fileItemRepo;

    public RepositoryWrapper(FileSharingDbContext dbContext)
    {
        _dbContext = dbContext; 
    }

    public IFileItemRepository fileItemRepo {
        get 
        { 
            if( _fileItemRepo == null ){
                _fileItemRepo = new FileItemRepository(_dbContext);
            }
            return _fileItemRepo;
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}