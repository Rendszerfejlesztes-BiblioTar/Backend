using BiblioBackend.DataContext.Entities;
namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Provides the user privilige string
/// </summary>
public class UserGetPrivilegeLevelDTO
{
    public PrivilegeLevel Privilege { get; set; }
    
    public string PrivilegeString { get; set; }
}