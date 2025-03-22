namespace BiblioBackend.DataContext.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Email { get; set; } // Nullable string
        public string? FirstName { get; set; } // Nullable string
        public string? LastName { get; set; } // Nullable string
        public string? PasswordHash { get; set; } // Nullable string
        public string? Phone { get; set; } // Nullable string
        public string? Address { get; set; } // Nullable string
        public PrivilegeLevel Privilege { get; set; }
        public List<Reservation> Reservations { get; set; } = new List<Reservation>(); // Inicializ�l�s
        public List<LoanHistory> LoanHistories { get; set; } = new List<LoanHistory>(); // Inicializ�l�s
    }
}