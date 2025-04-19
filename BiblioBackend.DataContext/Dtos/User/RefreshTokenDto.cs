using System.ComponentModel.DataAnnotations;

namespace BiblioBackend.BiblioBackend.DataContext.Dtos.User
{
    public class RefreshTokenDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
