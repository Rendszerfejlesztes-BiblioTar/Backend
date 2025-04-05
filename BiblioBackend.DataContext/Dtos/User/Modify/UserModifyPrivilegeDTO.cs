using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Modify the users privilige level
/// </summary>
public class UserModifyPrivilegeDTO
{
    public string RequesterEmail { get; set; }
    
    public string UserEmail { get; set; }
    public PrivilegeLevel NewPrivilege { get; set; }
    
    //TODO: Implement some sort of verification, that this action is being performed by an admin!!!!!!
    public string ChangeToken { get; set; }
}