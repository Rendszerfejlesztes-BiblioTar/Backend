using BiblioBackend.BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Entities
{
    public class User
    {
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PasswordHash { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public PrivilegeLevel Privilege { get; set; }
        public List<Reservation>? Reservations { get; set; }
        public List<Loan>? Loans { get; set; }
    }
}