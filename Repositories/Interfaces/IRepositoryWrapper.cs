public interface IRepositoryWrapper
{
   IFileItemRepository fileItemRepo{ get; }

   IFileItemAccessRepository fileItemAccessRepo { get; }

   IUserRepository userRepo { get; }

   IRefreshTokenRepository refreshTokenRepo {get;}

   Task SaveAsync();
}