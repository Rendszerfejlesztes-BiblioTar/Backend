using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication
/// </summary>
public class UserLoginTokenDto
{
    public string? AuthToken { get; set; }
}