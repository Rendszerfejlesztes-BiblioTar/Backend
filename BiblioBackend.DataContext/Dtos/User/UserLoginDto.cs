using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication
/// </summary>
public class UserLoginDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    
    public UserDto? User { get; set; }
}