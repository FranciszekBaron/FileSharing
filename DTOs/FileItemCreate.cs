public class FileItemCreate
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; } = DateTime.Now;
    public string? ParentId { get; set; }
}