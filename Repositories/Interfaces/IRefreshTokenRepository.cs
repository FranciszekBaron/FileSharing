public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
    Task<IEnumerable<RefreshToken>> GetAllByUserId(string userId);

    Task<RefreshToken> GetByToken(string token); 
    void DeleteRange(IEnumerable<RefreshToken> tokens);
}