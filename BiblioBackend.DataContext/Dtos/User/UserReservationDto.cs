namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Provides the reservations of the given user
/// </summary>
public class UserReservationDto
{
    public int? ReservationId { get; set; }
    
    public int? BookId { get; set; }
    
    public bool? IsAccepted { get; set; }
    
    public DateTime? ReservationDate { get; set; }
    
    public DateTime? ExpectedStart { get; set; }
    
    public DateTime? ExpectedEnd { get; set; }
}