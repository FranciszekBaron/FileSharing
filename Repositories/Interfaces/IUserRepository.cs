public interface IUserRepository : IRepositoryBase<User>
{
    Task<User> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id);
}