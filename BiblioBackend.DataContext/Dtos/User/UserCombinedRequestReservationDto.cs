namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Combines the needed parameters for a request
/// </summary>
public class UserCombinedRequestReservationDto
{
    public string Email { get; set; }
    public List<UserReservationDto> UserReservationDto { get; set; }
}