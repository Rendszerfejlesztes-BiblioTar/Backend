using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BiblioBackend.BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BiblioBackend.DataContext.Entities
{
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public PrivilegeLevel Privilege { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        [JsonIgnore]
        public List<Reservation>? Reservations { get; set; }

        [JsonIgnore]
        public List<Loan>? Loans { get; set; }
    }
}