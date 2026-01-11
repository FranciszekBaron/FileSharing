
public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly FileSharingDbContext _dbContext;

    private IFileItemRepository _fileItemRepo;

    private IFileItemAccessRepository _fileItemAccessRepo;

    

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

    public IFileItemAccessRepository fileItemAccessRepo
    {
        get
        {
            if( _fileItemAccessRepo == null)
            {
                _fileItemAccessRepo = new FileItemAccessRepository( _dbContext);
            }
            return _fileItemAccessRepo;
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}