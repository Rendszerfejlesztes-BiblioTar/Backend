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
}