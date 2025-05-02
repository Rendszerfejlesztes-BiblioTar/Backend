namespace BiblioBackend.BiblioBackend.Services
{
    public class ReservationPatchDTO
    {
        public int? BookId { get; set; }
        public string? UserEmail { get; set; }
        public bool? IsAccepted { get; set; }
        public DateTime? ReservationDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedStart { get; set; }
        public DateTime? ExpectedEnd { get; set; }
        
    }
}