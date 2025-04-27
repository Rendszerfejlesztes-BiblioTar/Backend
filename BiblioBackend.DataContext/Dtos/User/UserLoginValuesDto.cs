using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.DataContext.Dtos.User.Post;

/// <summary>
/// Stores information used for user authentication or creation
/// </summary>
public class UserLoginValuesDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}