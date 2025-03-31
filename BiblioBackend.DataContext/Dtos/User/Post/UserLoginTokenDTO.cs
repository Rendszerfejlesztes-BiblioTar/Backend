namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication
/// </summary>
public class UserLoginTokenDTO
{
    public string? AuthToken { get; set; }
}