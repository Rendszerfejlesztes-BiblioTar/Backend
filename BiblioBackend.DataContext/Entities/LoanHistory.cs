namespace BiblioBackend.DataContext.Entities
{
    public class LoanHistory
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; } // Nullable referencia
        public string? UserEmail { get; set; } // Nullable string
        public User? User { get; set; } // Nullable referencia
        public DateTime LoanDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReturnDate { get; set; } // Nullable DateTime
    }
}