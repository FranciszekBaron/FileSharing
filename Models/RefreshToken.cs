public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Token { get; set; } 
    public string UserId { get; set; }
    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;

    //Nav property
    public User User { get; set; }
}