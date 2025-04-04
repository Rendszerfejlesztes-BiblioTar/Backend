namespace BiblioBackend.DataContext.Dtos.User;

/// <summary>
/// Combines the needed parameters for a request
/// </summary>
public class UserCombinedRequestReservationDTO
{
    public string Email { get; set; }
    public List<UserReservationDTO> UserReservationDto { get; set; }
}