using FileSharing.Models;

public class FileItemAccess
{
    public string Id { get; set; } = Guid.NewGuid().ToString();


    //FK
    public string UserId { get; set; } = string.Empty;
    public string FileItemId { get; set; } = string.Empty;


    public string PermissionType { get; set; } = "read";  //owner/read/edit
    public DateTime SharedDate { get; set; } = DateTime.Now;


    //Navigation znowy mamy id, ale zeby nie robic lazy loading 
    public User User { get; set; } = null!;
    public FileItem FileItem { get; set; } = null!;

}