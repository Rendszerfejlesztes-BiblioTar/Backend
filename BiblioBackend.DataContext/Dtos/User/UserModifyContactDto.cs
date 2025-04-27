using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Used to change the users contact information, excluding the email
/// </summary>
public class UserModifyContactDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public string? Address { get; set; }
}