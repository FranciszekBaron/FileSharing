using System.Security.Claims;
using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileItemController : ControllerBase
{
    public string user = "user1";
    private readonly IFileService _fileItemService;

    public FileItemController(IFileService fileItemService)
    {
        _fileItemService = fileItemService;
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


    [HttpGet(Name = "GetAllUserFiles")]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        try {
            var user = GetUserIdFromToken();
            return Ok(await _fileItemService.GetAllFilesAsync(user));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  // ✅ Dodaj new { error = ... }
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

    [HttpGet("{fileId}",Name = "GetFileById")]
    [Authorize]
    public async Task<IActionResult> GetById(string fileId)
    {
        
        try
        {
            var user = GetUserIdFromToken();
            var file = await _fileItemService.GetFileById(fileId, user);
            return Ok(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{fileId}/downloadFile",Name = "Download")]
    [Authorize]

    public async Task<IActionResult> DownloadFileItem(string fileId)
    {
        try
        {
            var user = GetUserIdFromToken();
            var response = await _fileItemService.DownloadFileAsync(fileId,user);
            return File(response.fileStream,response.contentType,response.fileName);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpGet("allSharedUser", Name = "GetAllSharedUsers")]
    [Authorize]

    public async Task<IActionResult> GetAllSharedUsers()
    {
        try {
            var user = GetUserIdFromToken();
            var sharedUsers = await _fileItemService.GetAllSharedUsers(user);
            return Ok(sharedUsers);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  // ✅ Dodaj new { error = ... }
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

    [HttpGet("{fileId}/usersWithAccess",Name ="GetUsersWithAccess")]
    [Authorize]

    public async Task<IActionResult> GetUsersWithAccess(string fileId)
    {
        try {
            var user = GetUserIdFromToken();
            var usersWithAccess = await _fileItemService.GetUsersWithAccess(fileId,user);
            return Ok(usersWithAccess);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  // ✅ Dodaj new { error = ... }
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

    [HttpPost("folder", Name = "CreateFolder")]
    [Authorize]

    public async Task<IActionResult> CreateFileItem([FromBody] FolderCreate body)
    {
        var user = GetUserIdFromToken();
        //TODO token jwt   
        try
        {
            var file = await _fileItemService.CreateFolderAsync(body, user);
            return Ok(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("uploadFile", Name = "Upload")]
    [Authorize]

    public async Task<IActionResult> UploadFileItem([FromForm] FileUploadDto body)
    {   
        var user = GetUserIdFromToken();
        Console.WriteLine("Wykonuje endpoint Upload");
        try
        {
            var file = await _fileItemService.UploadFileAsync(user,body);
            return Ok(file);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.ResetColor();
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpPost("{fileId}/share", Name = "Share")]
    [Authorize]
    

    public async Task<IActionResult> Share(string fileId, [FromBody] FileItemAccessCreate dto)
    {
        Console.WriteLine("Wykonuje endpoint SHARe");
        var user = GetUserIdFromToken();
        try {
            var file = await _fileItemService.ShareFileAsync(fileId,user,dto);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { error = e.Message });  // ✅ Dodaj new { error = ... }
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



    [HttpDelete("{fileId}/softDelete", Name = "SoftDelete")]
    [Authorize]

    public async Task<IActionResult> SoftDeleteFileItem(string fileId)
    {
        var user = GetUserIdFromToken();
        try {
            var file = await _fileItemService.SoftDeleteFileAsync(fileId,user);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();  // 403 Forbidden
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // 400 Bad Request
        }
    }

    [HttpDelete("{fileId}/permanentDelete", Name = "PermanentDelete")]
    [Authorize]

    public async Task<IActionResult> PermanentFileDelete(string fileId)
    {
        var user = GetUserIdFromToken();
        try {
            var file = await _fileItemService.PermanentFileDeleteAsync(fileId,user);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();  // 403 Forbidden
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // 400 Bad Request
        }
    }

    [HttpPatch("{fileId}/restore", Name = "Restore")]
    [Authorize]
    public async Task<IActionResult> RestoreFileItem(string fileId)
    {
        var user = GetUserIdFromToken();
        try {
            var file = await _fileItemService.RestoreFileAsync(fileId,user);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();  // 403 Forbidden
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // 400 Bad Request
        }
    }


    [HttpPatch("{fileId}/toggleStarred", Name = "ToggleStarred")]
    [Authorize]

    public async Task<IActionResult> ToggleStarred(string fileId)
    {
        var user = GetUserIdFromToken();

        try {
            var file = await _fileItemService.ToggleStarredAsync(fileId,user);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();  // 403 Forbidden
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);  // 400 Bad Request
        }
    }

    [HttpPatch("{fileId}/rename", Name = "Rename")]
    [Authorize]
    public async Task<IActionResult> Rename(string fileId,[FromBody] FileRename body)
    {
        var user = GetUserIdFromToken();
        try {
            var file = await _fileItemService.RenameAsync(fileId,user,body);
            return Ok(file);
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();  // 403 Forbidden
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message + "blablabla");  // 400 Bad Request
        }
    }
}
