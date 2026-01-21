using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FileSharing.Controllers;


[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    
    private readonly IAuthService _authService;

    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    private string GetUserIdFromToken()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value                   // JWT sub
                     ?? User.FindFirst("userId")?.Value;  
        
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User ID not found in token");
            
        return userId;
    }

    [HttpPost("refreshToken",Name = "RefreshToken")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh token not found" });
        }

        var response = await _authService.RefreshTokenAsync(refreshToken);

        SetAccessTokenCookie(response.AccessToken,response.ExpiresAt);

        if (!string.IsNullOrEmpty(response.RefreshToken) && response.RefreshToken != refreshToken)
        {
            SetRefreshTokenCookie(response.RefreshToken);
        }

        return Ok(new { message = "Token refreshed successfully" });
        
    }


    [HttpPost("login",Name = "Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        
        var response = await _authService.LoginAsync(loginRequest);


        SetAccessTokenCookie(response.AccessToken, response.ExpiresAt);

        SetRefreshTokenCookie(response.RefreshToken);

        return Ok(new
        {
            message = "Login successful",
            user = new
            {
                id = response.User?.Id,
                email = response.User?.Email,
                userName = response.User?.UserName,
                avatar = response.User?.Avatar
            }
        });
        
    }

    [HttpGet("me",Name = "Me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserIdFromToken();
        var responseUser = await _authService.MeAsync(userId);

        return Ok(new
        {
            id = responseUser.Id,
            email = responseUser.Email,
            userName = responseUser.UserName,
            avatar = responseUser.Avatar
        });
    }


    [HttpDelete("logout",Name = "Logout")] 
    [Authorize]
    public async Task<IActionResult> Logout()
    {

        var refreshToken = Request.Cookies["refreshToken"];


        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "Refresh token not found" });
        }

        await _authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete("refreshToken");
        Response.Cookies.Delete("accessToken");

        return Ok(new { message = "Token deleted" });
        
    } 


    [HttpPatch("logoutFromAllDevices",Name="LogoutFromAllDevices")]
    [Authorize]
    public async Task<IActionResult> LogoutFromAllDevices()
    {
        var user = GetUserIdFromToken();
        var response = await _authService.LogoutAllAsync(user);

        Response.Cookies.Delete("refreshToken");
        Response.Cookies.Delete("accessToken");

        return Ok(new { message = "Token deleted" });
    } 


    private void SetAccessTokenCookie(string accessToken,DateTime expiresAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax, // ochrona przed CRSF, ale Lax bo local
            Expires = expiresAt,
            Path = "/"
        };

        Response.Cookies.Append("accessToken",accessToken,cookieOptions);
    }


    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,  
            SameSite = SameSiteMode.Lax,  // Lax nie Strict na lokal 
            Expires = DateTime.UtcNow.AddDays(
                _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays")
            ),
            Path = "/"
        };

        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}