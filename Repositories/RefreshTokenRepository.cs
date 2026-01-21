

using System.Collections;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository :  RepositoryBase<RefreshToken>, IRefreshTokenRepository
{

    private readonly FileSharingDbContext _dbContext;
    public RefreshTokenRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
            _dbContext = dbContext;
    }

    public async Task<IEnumerable<RefreshToken>> GetAllByUserId(string userId)
    {
        return await  GetByConditionAsync(e => e.UserId == userId);
    }

    public async Task<RefreshToken?> GetByToken(string token)
    {
        return await _dbContext.RefreshToken
        .Include(t => t.User) // ✅ Załaduj User
        .FirstOrDefaultAsync(t => t.Token == token);
    }

    public void DeleteRange(IEnumerable<RefreshToken> tokens)
    {
        _dbContext.RefreshToken.RemoveRange(tokens);
    }
}