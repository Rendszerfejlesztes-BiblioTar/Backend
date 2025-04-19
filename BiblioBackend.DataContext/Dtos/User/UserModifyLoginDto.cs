using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Used for modifing the users email address and password hash
/// </summary>
public class UserModifyLoginDto
{
    [Required, EmailAddress]
    public string OldEmail { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string NewEmail { get; set; } = string.Empty;

    [MinLength(8)]
    public string? NewPassword { get; set; }
}