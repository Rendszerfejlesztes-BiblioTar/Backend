namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Used for modifing the users email address
/// </summary>
public class UserModifyEmailDTO
{
    /// <summary>
    /// The email in the database
    /// </summary>
    public string OldEmail { get; set; }
    
    /// <summary>
    /// The email which is not in the database, proper checks must be done!!
    /// </summary>
    public string NewEmail { get; set; }
}