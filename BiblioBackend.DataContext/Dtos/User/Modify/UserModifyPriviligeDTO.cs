using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Modify the users privilige level
/// </summary>
public class UserModifyPriviligeDTO
{
    public string Email { get; set; }
    public PrivilegeLevel NewPrivilege { get; set; }
    
    //TODO: Implement some sort of verification, that this action is being performed by an admin!!!!!!
    public string ChangeToken { get; set; }
}