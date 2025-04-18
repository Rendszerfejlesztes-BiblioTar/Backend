namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication or creation
/// </summary>
public class UserLoginValuesDto
{
    public string Email { get; set; }
    
    /// <summary>
    /// Hashed on the frontend? should not send password in plaintext!!!!
    /// </summary>
    public string PasswordHash { get; set; }
}