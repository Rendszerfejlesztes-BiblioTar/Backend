using BiblioBackend.BiblioBackend.Services;
using BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservationsAsync()
        {
            var result = await _reservationService.GetAllReservationsAsync();
            return Ok(result);
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUsersReservationsAsync(string email)
        {
            var result = await _reservationService.GetUsersReservationsAsync(email);
            return Ok(result);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllInfoForReservationAsync(int id)
        {
            var result = await _reservationService.GetAllInfoForReservationAsync(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync([FromBody] ReservationPostDTO reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _reservationService.CreateReservationAsync(reservation);
            return Ok(result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReservationAsync(int id, [FromBody] ReservationPatchDTO reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _reservationService.UpdateReservationAsync(id, reservation);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationAsync(int id)
        {
            var result = await _reservationService.DeleteReservationAsync(id);

            if (result)
            {
                return Ok(result);
            }

            return NotFound();
        }
    }
}
