public class UserGetDto
{
    public string UserName { get; set;} = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; } //URL lub base64
}