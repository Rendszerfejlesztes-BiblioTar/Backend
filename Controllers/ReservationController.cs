using BiblioBackend.BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.BiblioBackend.Services;
using BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        
        // ---
        
        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        private ObjectResult MissingReservation => NotFound("A kért foglalás nem létezik!");
        
        // ---
        
        /// <summary>
        /// Get all reservations in the database
        /// </summary>
        /// <returns>A list of reservation dtos</returns>
        [Authorize] //Since this gives back all the loans in the db currently, basic security measure is that this is only permitted for librarian and up
        [HttpGet]
        public async Task<IActionResult> GetAllReservationsAsync([FromBody] ReservationGetAllDto reservationGetAllDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, reservationGetAllDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, reservationGetAllDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.GetAllReservationsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get every reservation tied to an email
        /// </summary>
        /// <param name="email">The email to get the reservations from</param>
        /// <returns>The list of reservations tied to that email</returns>
        [Authorize]
        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUsersReservationsAsync(string email)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.GetUsersReservationsAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Obtain all related information tied to a reservation
        /// </summary>
        /// <param name="reservationGetInfoIdDto">The dto from which we fetch the data from</param>
        /// <returns>Detailed info about the reservation</returns>
        [Authorize]
        [HttpGet("byid")]
        public async Task<IActionResult> GetAllInfoForReservationAsync([FromBody] ReservationGetInfoIdDto reservationGetInfoIdDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, reservationGetInfoIdDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, reservationGetInfoIdDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.GetAllInfoForReservationAsync(reservationGetInfoIdDto.Id);
            return Ok(result);
        }
        
        /// <summary>
        /// Create a new reservation
        /// </summary>
        /// <param name="reservation">The dto to create the reservation from</param>
        /// <returns>The dto containing the reservation data</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateReservationAsync([FromBody] ReservationPostDTO reservation)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, reservation.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, reservation.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.CreateReservationAsync(reservation);
            return Ok(result);
        }

        /// <summary>
        /// Update an existing reservation
        /// </summary>
        /// <param name="id">Id of the reservation</param>
        /// <param name="reservation">The dto of the data to update from</param>
        /// <returns>The updated dto data</returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateReservationAsync(int id, [FromBody] ReservationPatchDTO reservation)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, reservation.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, reservation.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.UpdateReservationAsync(id, reservation);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete an existing reservation
        /// </summary>
        /// <param name="reservationDeleteDto">Data related to the reservation to delete</param>
        /// <returns>True if deleted</returns>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteReservationAsync([FromBody] ReservationDeleteDto reservationDeleteDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, reservationDeleteDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, reservationDeleteDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _reservationService.DeleteReservationAsync(reservationDeleteDto.Id);

            if (!result)
                return MissingReservation;

            return Ok(result);
        }
    }
}
