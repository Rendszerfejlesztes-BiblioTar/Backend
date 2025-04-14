using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication
/// </summary>
public class UserLoginTokenDTO
{
    public string? AuthToken { get; set; }
    public string? Email { get; set; }
    public PrivilegeLevel Privilege { get; set; }
}