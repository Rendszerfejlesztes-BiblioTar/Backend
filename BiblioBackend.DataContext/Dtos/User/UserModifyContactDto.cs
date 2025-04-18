namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Used to change the users contact information, excluding the email
/// </summary>
public class UserModifyContactDto
{
    public string Email { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
}