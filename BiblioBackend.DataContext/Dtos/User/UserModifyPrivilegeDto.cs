using BiblioBackend.DataContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Modify the users privilige level
/// </summary>
public class UserModifyPrivilegeDto
{
    [Required, EmailAddress]
    public string RequesterEmail { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string UserEmail { get; set; } = string.Empty;

    [Required]
    public PrivilegeLevel NewPrivilege { get; set; }
}