namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Provides core information about loans
/// </summary>
public class UserLoanDTO
{
    public int LoanId { get; set; }
    
    public int BookId { get; set; }
    
    public int Extensions { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime ExpectedEndDate { get; set; }
    
    public DateTime? ReturnDate { get; set; }
}