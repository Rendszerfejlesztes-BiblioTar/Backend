using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.BiblioBackend.DataContext.Entities
{
    public class Loan
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public User? User { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int Extensions { get; set; } = 2;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime ExpectedEndDate { get; set; } 
        public DateTime? ReturnDate { get; set; }
    }
}
