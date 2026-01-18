

using System.Collections;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository :  RepositoryBase<RefreshToken>, IRefreshTokenRepository
{

    private readonly FileSharingDbContext _dbContext;
    public RefreshTokenRepository(FileSharingDbContext dbContext) : base(dbContext)
    {
            _dbContext = dbContext;
    }

    public async Task<IEnumerable<RefreshTokenGetDto>> GetAllByUserId(string userId)
    {
        var tokens = await  GetByConditionAsync(e => e.UserId == userId);

        return tokens.Select(t => new RefreshTokenGetDto
        {
            Token = t.Token,
            UserId = t.UserId,
            ExpiresAt = t.ExpiresAt,
            CreatedAt = t.CreatedAt,
            IsRevoked = t.IsRevoked
        });
    }

    public async Task<RefreshToken?> GetByToken(string token)
    {
        return await _dbContext.RefreshToken
        .Include(t => t.User) // ✅ Załaduj User
        .FirstOrDefaultAsync(t => t.Token == token);
    }
}