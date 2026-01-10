public class FileItemGet
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long? Size { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? ParentId { get; set; }
    public bool Starred { get; set; }
    public bool Deleted { get; set; }
    public string Permission { get; set; } = string.Empty;
}