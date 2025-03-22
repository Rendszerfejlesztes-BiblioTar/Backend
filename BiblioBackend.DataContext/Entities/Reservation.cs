namespace BiblioBackend.DataContext.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; } // Nullable referencia
        public string? UserEmail { get; set; } // Nullable string
        public User? User { get; set; } // Nullable referencia
        public bool IsAllowed { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
    }
}