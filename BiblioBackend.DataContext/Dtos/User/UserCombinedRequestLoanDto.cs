namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Combines the needed parameters for a request
/// </summary>
public class UserCombinedRequestLoanDto
{
    public string Email { get; set; }
    public List<UserLoanDto> UserLoanDto { get; set; }
}