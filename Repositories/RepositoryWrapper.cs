
public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly FileSharingDbContext _dbContext;

    private IFileItemRepository _fileItemRepo;

    private IFileItemAccessRepository _fileItemAccessRepo;
    
    private IRefreshTokenRepository _refreshTokenRepo;

    private IUserRepository _userRepo;

    

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

    public IUserRepository userRepo
    {
        get
        {
            if(_userRepo == null)
            {
                _userRepo = new UserRepository(_dbContext);
            }
            return _userRepo;
        }
    }

    public IRefreshTokenRepository refreshTokenRepo
    {
        get
        {
            if(_refreshTokenRepo == null)
            {
                _refreshTokenRepo = new RefreshTokenRepository(_dbContext);
            }
            return _refreshTokenRepo;
        }
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}