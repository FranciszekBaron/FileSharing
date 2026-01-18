using FileSharing.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set;} = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; } //URL lub base64
    public string PasswordHash { get; set; } = string.Empty;


    //Navigation Properties, eager loading -> robi sie go po to zeby odzwierciedlic w kodzie obiekt z bazy odrazu, a nie 
    //wysylac osobne zapytanie po np Ownera tego File, wtedy 2 queries zamiast jednego, ale przy loopie 100 dodatkowych queries np. N+1 problem
    public ICollection<FileItem> OwnedFiles { get; set; } = new List<FileItem>();
    public ICollection<FileItemAccess> FileAccesses { get; set; } = new List<FileItemAccess>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}