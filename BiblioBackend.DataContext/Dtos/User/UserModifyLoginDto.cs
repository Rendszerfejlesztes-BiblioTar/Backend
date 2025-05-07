using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Used for modifing the users email address and password hash
/// </summary>
public class UserModifyLoginDto
{
    [MinLength(8)]
    public string? NewPassword { get; set; }
}