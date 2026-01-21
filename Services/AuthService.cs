
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FileSharing.Services;

public class AuthService : IAuthService
{
    private readonly IRepositoryWrapper _repository;
    private readonly IConfiguration _configuration;

    public AuthService(IRepositoryWrapper repository,IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }



    public async Task<AuthResponse> LoginAsync(LoginRequest loginRequest)
    {
        var user = await _repository.userRepo.GetByEmailAsync(loginRequest.Email);

        if(user == null)
            throw new KeyNotFoundException($"User with email: {loginRequest.Email} not found");


        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash);
        if(!isPasswordValid) 
            throw new UnauthorizedAccessException("Invalid email or password");

        var oldTokens = await _repository.refreshTokenRepo.GetAllByUserId(user.Id);
        _repository.refreshTokenRepo.DeleteRange(oldTokens);

        var accessToken = GenerateAccessToken(user);

        
        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        _repository.refreshTokenRepo.Create(refreshToken);
        await _repository.SaveAsync();
        
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes")),
            User = new UserAuthDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Avatar = user.Avatar
            },
        }; 
    }


    private string GenerateAccessToken(User user)
    {

        var secretKey = _configuration["Jwt:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID tokenu
            new Claim("userId", user.Id), // Custom claim
            new Claim("name", user.UserName),
            new Claim("avatar", user.Avatar ?? ""),
        };


    
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes")
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> LogoutAllAsync(string userId)
    {
        var user = await _repository.userRepo.GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException ("User not found");
        
        var tokensToDelete = await _repository.refreshTokenRepo.GetAllByUserId(user.Id);

        // ✅ Soft delete - oznacz jako unieważnione
        foreach (var token in tokensToDelete)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        await _repository.SaveAsync();
        return true;
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var token = await _repository.refreshTokenRepo.GetByToken(refreshToken);

        if(token == null)
            throw new KeyNotFoundException("Token not found");

        _repository.refreshTokenRepo.Delete(token);
        await _repository.SaveAsync();

        return true;
    }

    
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _repository.refreshTokenRepo.GetByToken(refreshToken);

        if(token == null)
            throw new KeyNotFoundException("Token not found");

        var accessToken = GenerateAccessToken(token.User);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = token.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes")),
            User = new UserAuthDto
            {
                Id = token.User.Id,
                UserName = token.User.UserName,
                Email = token.User.Email,
                Avatar = token.User.Avatar
            }
        }; 
    }


    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        // Sprawdź czy email już istnieje
        var existingUser = await _repository.userRepo.GetByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email already exists");
        
        // Hash hasła
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = request.Email,
            UserName = request.UserName,
            PasswordHash = passwordHash, // ✅ Zapisz hash, nie plaintext!
            Avatar = request.Avatar
        };
        
        _repository.userRepo.Create(user);
        await _repository.SaveAsync();
        
        return user;
    }

    public async Task<User> MeAsync(string userId)
    {
        var me = await _repository.userRepo.GetByIdAsync(userId);

        if(me == null)
            throw new KeyNotFoundException("User not found");

        return me;
    }
}