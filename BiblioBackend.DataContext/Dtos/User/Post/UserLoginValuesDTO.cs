namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication
/// </summary>
public class UserLoginValuesDTO
{
    public string Email { get; set; }
    
    /// <summary>
    /// Hashed on the frontend? should not send password in plaintext!!!!
    /// </summary>
    public string PasswordHash { get; set; }
}