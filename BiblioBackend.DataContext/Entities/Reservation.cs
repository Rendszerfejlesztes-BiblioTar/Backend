namespace BiblioBackend.DataContext.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public string UserEmail { get; set; }
        public User? User { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpectedStart { get; set; }
        public DateTime ExpectedEnd { get; set; }
    }
}