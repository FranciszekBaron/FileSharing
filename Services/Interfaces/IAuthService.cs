public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest loginRequest);

    Task<AuthResponse> RefreshTokenAsync(string refreshToken);

    Task<bool> LogoutAsync(string refreshToken);

    Task<bool> LogoutAllAsync(string userId);
}   