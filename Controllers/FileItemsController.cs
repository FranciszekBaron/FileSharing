using FileSharing.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileSharing.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileItemController : ControllerBase
{
    private readonly IFileService _fileItemService;

    public FileItemController(IFileService fileItemService)
    {
        _fileItemService = fileItemService;
    }

    [HttpGet(Name = "GetAllUserFiles")]
    public async Task<IActionResult> Get()
    {

        //TODO token jwt 
        var userId = "user1";
        return Ok(await _fileItemService.GetAllFilesAsync(userId));
    }


    [HttpPost(Name = "CreateItem")]
    public async Task<IActionResult> CreateFileItem(FileItemCreate body)
    {

        //TODO token jwt 
        var userId = "user1";
        try
        {
            var file = await _fileItemService.CreateFileAsync(body, userId);
            return Ok(file);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{fileId}", Name = "SoftDelete")]
    public async Task<IActionResult> SoftDeleteFileItem(string fileId){
        
        var userId = "user1";

        try {
            var file = await _fileItemService.SoftDeleteFileAsync(fileId,userId);
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
