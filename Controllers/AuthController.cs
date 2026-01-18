using Microsoft.AspNetCore.Mvc;
namespace FileSharing.Controllers;


[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("refreshToken",Name = "RefreshToken")]
    public async Task<IActionResult> RefreshToken(string refreshToken)
    {

        try
        {
            var token = await _authService.RefreshTokenAsync(refreshToken);

            return Ok(token);
        }
    
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(403, new { error = e.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });  // ✅ Dodaj new { error = ... }
        }
    }


    [HttpPost("login",Name = "Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var response = await _authService.LoginAsync(loginRequest);

            return Ok(response);
        }
    
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(403, new { error = e.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });  // ✅ Dodaj new { error = ... }
        }
    }


    [HttpDelete("logout",Name = "Logout")] 

    public async Task<IActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            var response = await _authService.LogoutAsync(refreshToken);

            return Ok(response);
        }
    
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  
        }
        catch (UnauthorizedAccessException e)
        {
            return StatusCode(403, new { error = e.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });  // ✅ Dodaj new { error = ... }
        }
    } 










}