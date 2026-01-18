public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
    Task<IEnumerable<RefreshTokenGetDto>> GetAllByUserId(string userId);

    Task<RefreshToken> GetByToken(string token); 
}