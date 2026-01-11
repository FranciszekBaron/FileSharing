using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileItemController : ControllerBase
{

    public string userIdmock = "user1";
    private readonly IFileService _fileItemService;

    public FileItemController(IFileService fileItemService)
    {
        _fileItemService = fileItemService;
    }

    [HttpGet(Name = "GetAllUserFiles")]
    public async Task<IActionResult> Get()
    {

        //TODO token jwt 
    
        return Ok(await _fileItemService.GetAllFilesAsync(userIdmock));
    }


    [HttpPost("folder", Name = "CreateFolder")]
    public async Task<IActionResult> CreateFileItem(FolderCreate body)
    {

        //TODO token jwt 
        
        try
        {
            var file = await _fileItemService.CreateFolderAsync(body, userIdmock);
            return Ok(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{fileId}", Name = "SoftDelete")]
    public async Task<IActionResult> SoftDeleteFileItem(string fileId){
        
        

        try {
            var file = await _fileItemService.SoftDeleteFileAsync(fileId,userIdmock);
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

    public async Task<IActionResult> RestoreFileItem(string fileId)
    {
        

        try {
            var file = await _fileItemService.RestoreFileAsync(fileId,userIdmock);
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

    public async Task<IActionResult> ToggleStarred(string fileId)
    {
        var userId = "user1";

        try {
            var file = await _fileItemService.ToggleStarredAsync(fileId,userIdmock);
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

    public async Task<IActionResult> Rename(string fileId,[FromBody] FileRename body)
    {
        try {
            var file = await _fileItemService.RenameAsync(fileId,userIdmock,body);
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


    [HttpPost("{fileId}/share", Name = "Share")]

    public async Task<IActionResult> Share(string fileId, [FromBody] FileItemAccessCreate dto)
    {
        try {
            var file = await _fileItemService.ShareFileAsync(fileId,dto.UserId,dto.Permission);
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


}
