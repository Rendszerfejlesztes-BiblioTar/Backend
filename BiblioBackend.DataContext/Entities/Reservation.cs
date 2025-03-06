namespace BiblioBackend.DataContext.Entities;

public class Reservation
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; }

    public string UserEmail { get; set; }
    public User User { get; set; }

    public bool IsAllowed { get; set; }

    public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
}