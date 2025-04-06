namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Combines the needed parameters for a request
/// </summary>
public class UserCombinedRequestLoanDTO
{
    public string Email { get; set; }
    public List<UserLoanDTO> UserLoanDto { get; set; }
}