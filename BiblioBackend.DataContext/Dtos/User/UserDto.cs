using System.ComponentModel.DataAnnotations;
using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos
{
    public class UserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        public PrivilegeLevel Privilege { get; set; }

        public string PrivilegeString => Privilege.ToFriendlyString();
    }


}