using BiblioBackend.DataContext.Entities;
namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Provides the user privilige string
/// </summary>
public class UserGetPriviligeLevelDTO
{
    public PrivilegeLevel Privilige { get; set; }
    
    public string PriviligeString { get; set; }
}