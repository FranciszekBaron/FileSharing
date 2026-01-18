
public class UserRepository : RepositoryBase<User>, IUserRepository
{

    private readonly FileSharingDbContext _dbContext;
    public UserRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return _dbContext.Users.FirstOrDefault(x => x.Email == email);    
    }
}