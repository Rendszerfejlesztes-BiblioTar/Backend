using BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.DataContext.Dtos.User.Post;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers the user
        /// </summary>
        /// <param name="userLoginValuesDto">The values from which the new user will be created</param>
        /// <returns>The requested actions result (true for success)</returns>
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] UserLoginValuesDTO userLoginValuesDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Is user exists with this email?
            var isExists = await _userService.GetUserIsExistsAsync(userLoginValuesDto.Email);
            if (isExists)
                return BadRequest("Ez az email már regisztrálva van.");

            var result = await _userService.PostUserCreateAsync(userLoginValuesDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <param name="userLoginValuesDto">The values from which the user will be authenticated</param>
        /// <returns>The token of the authenticated user</returns>
        [HttpPost]
        public async Task<IActionResult> AuthenticateUser([FromBody] UserLoginValuesDTO userLoginValuesDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Is user exists with this email?
            var isExists = await _userService.GetUserIsExistsAsync(userLoginValuesDto.Email);
            if (!isExists)
                return BadRequest("A felhasználó nem létezik.");

            var result = await _userService.PostAutenticationAsync(userLoginValuesDto);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the user, where the given email is tied to
        /// </summary>
        /// <param name="email">The email of the given user</param>
        /// <returns>The users contact information</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserContact([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");

            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.GetUserContactInformationByEmailAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Check if the given user is authenticated or not
        /// </summary>
        /// <param name="email">The email of the given user</param>
        /// <returns>True if the user is authenticated</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserIsAuthenticated([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");

            var result = await _userService.GetUserAuthenticatedAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get the priviliges of a given user
        /// </summary>
        /// <param name="email">The email of the given user</param>
        /// <returns>The users privilege information</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserPrivilege([FromBody] string email)
        {
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");

            var result = await _userService.GetUserPrivilegeLevelByEmailAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get all the users reservation information
        /// </summary>
        /// <param name="email">The email of the given user</param>
        /// <returns>All reservations tied to the user</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserReservations([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.GetUserReservationsByEmailAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get some of the users reservation information
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto containing information about the needed data</param>
        /// <returns>Selected reservations tied to the user</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserReservationsSelected([FromBody] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.GetUserSelectedReservationsByEmailAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Get all the users loan information
        /// </summary>
        /// <param name="email">The email of the given user</param>
        /// <returns>All loans tied to the user</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserLoans([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");
            
            var result = await _userService.GetUserLoansByEmailAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get some of the users loan information
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto containing information about the needed data</param>
        /// <returns>Selected loans tied to the user</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserLoansSelected([FromBody] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.GetUserSelectedLoansByEmailAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Change the users contact information
        /// </summary>
        /// <param name="userModifyContactDto">The dto from which the contact information will be updated from</param>
        /// <returns>The result of the action (true for success)</returns>
        [HttpPut]
        public async Task<IActionResult> PutUserNewContact([FromBody] UserModifyContactDTO userModifyContactDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userModifyContactDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userModifyContactDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.UpdateUserContactInformationAsync(userModifyContactDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Register a new email for the given user
        /// </summary>
        /// <param name="userModifyLoginDto">The dto from which the information will be updated from</param>
        /// <returns>The result of the action (true for success)</returns>
        [HttpPut]
        public async Task<IActionResult> PutUserNewEmail([FromBody] UserModifyLoginDTO userModifyLoginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userModifyLoginDto.OldEmail);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userModifyLoginDto.OldEmail);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.UpdateUserLoginAsync(userModifyLoginDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Add new reservations to the given user
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto list from which the reservations will be created</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpPut]
        public async Task<IActionResult> PutUserNewReservations([FromBody] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.UpdateUserReservationsAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Add new loans to the given user
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto list from which the loans will be created</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpPut]
        public async Task<IActionResult> PutUserNewLoans([FromBody] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.UpdateUserLoansAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Give the specified user new permission
        /// </summary>
        /// <param name="userModifyPrivilegeDto">The dto from which the information will be updated from</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpPut]
        public async Task<IActionResult> PutUserNewPermission([FromBody] UserModifyPrivilegeDTO userModifyPrivilegeDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userModifyPrivilegeDto.UserEmail) && await _userService.GetUserIsExistsAsync(userModifyPrivilegeDto.RequesterEmail);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userModifyPrivilegeDto.RequesterEmail);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.UpdateUserPrivilegeAsync(userModifyPrivilegeDto);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="email">The email of the user to delete</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] string email)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.RemoveUserAsync(email);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete the users given reservation information
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto from which the reservations will be removed by</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserReservations([FromBody] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.RemoveUserReservationAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete the users given loan information
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto from which the loans will be removed by</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserLoans([FromBody] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await _userService.GetUserIsExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return BadRequest("Nem létezik ilyen felhasználó!");
            
            var isAuthenticated = await _userService.GetUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return BadRequest("Nem vagy bejelentkezve!");

            var result = await _userService.RemoveUserLoansAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
    }
}