namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Stripped down version of the user entity, providing only the contact information of the user
/// </summary>
public class UserGetContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
}