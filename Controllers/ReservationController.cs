using BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.Services;
using BiblioBackend.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BiblioBackend.BiblioBackend.Services;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly IUserService _userService;

        public ReservationController(IReservationService reservationService, IUserService userService)
        {
            _reservationService = reservationService;
            _userService = userService;
        }

        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingReservation => NotFound("A kért foglalás nem létezik!");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllReservationsAsync()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.GetAllReservationsAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUsersReservationsAsync()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.GetUsersReservationsAsync(email);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAllInfoForReservationAsync(int id)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.GetAllInfoForReservationAsync(id);
                if (result == null) return MissingReservation;
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync([FromBody] ReservationPostDto reservation)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.CreateReservationAsync(reservation, email); // Pass email for UserEmail
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReservationAsync(int id, [FromBody] ReservationPatchDto reservation)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.UpdateReservationAsync(id, reservation, email); // Pass email for validation/auditing
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservationAsync(int id)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _reservationService.DeleteReservationAsync(id, email); // Pass email for validation/auditing
                if (!result) return MissingReservation;
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}