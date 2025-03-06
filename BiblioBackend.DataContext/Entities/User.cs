namespace BiblioBackend.DataContext.Entities;

public class User
{
    public int Id { get; set; }
    
    public string Email { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }

    public string PasswordHash { get; set; }

    public string Phone { get; set; }

    public string Address { get; set; }

    public PrivilegeLevel Privilege { get; set; }

    public ICollection<Reservation> Reservations { get; set; }
    
    public ICollection<LoanHistory> LoanHistories { get; set; }
}