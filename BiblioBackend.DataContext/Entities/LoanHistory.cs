namespace BiblioBackend.DataContext.Entities;

public class LoanHistory
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; }

    public string UserEmail { get; set; }
    public User User { get; set; }

    public DateTime LoanDate { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnDate { get; set; }
}