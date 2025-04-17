using BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.DataContext.Dtos.User.Post;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
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
        
        // --------------

        private ObjectResult ExistingUser => BadRequest("Ez az email már regisztrálva van.");
        private ObjectResult MissingUser => BadRequest("Nem létezik ilyen felhasználó!");
        private ObjectResult InvalidCredentials => BadRequest("Érvénytelen email, vagy jelszó!");
        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        
        // --------------

        /// <summary>
        /// Checks if the given emails are tied to users exists
        /// </summary>
        /// <param name="emails">The users emails</param>
        /// <returns>True if exists</returns>
        private async Task<bool> CheckIsUserExistsAsync(params string[] emails)
        {
            var flag = false;
            foreach (var email in emails)
            {
                flag = await _userService.GetUserIsExistsAsync(email);
                if (!flag) // one of the emails doesnt exists, meaning we dont pass all the checks 100%
                    return false;
            }
            return flag;
        }
        
        /// <summary>
        /// Checks if the given emails are authenticated
        /// </summary>
        /// <param name="emails">The users emails</param>
        /// <returns>True if authenticated</returns>
        private async Task<bool> CheckIsUserAuthenticatedAsync(params string[] emails)
        {
            var flag = false;
            foreach (var email in emails) // probably only will ever have 1 email in the params, but just in case
            {
                flag = await _userService.GetUserAuthenticatedAsync(email);
                if (!flag) // one of the emails doesnt exists, meaning we dont pass all the checks 100%
                    return false;
            }
            return flag;
        }
        
        /// <summary>
        /// Checks if the given permission level is enough to satisfy the needed privileges
        /// </summary>
        /// <param name="email">The users email</param>
        /// <param name="neededPrivileges">The privilege which is needed</param>
        /// <returns>True if permitted</returns>
        private async Task<bool> CheckIsUserPermittedAsync(string email, params PrivilegeLevel[] neededPrivileges)
        {
            PrivilegeLevel actualPrivilege = (await _userService.GetUserPrivilegeLevelByEmailAsync(email)).Privilege;
            PrivilegeLevel merged = neededPrivileges[0];
            foreach (var level in neededPrivileges) // probably only will ever have 1 email in the params, but just in case
            {
                merged |= level;
            }
            return (actualPrivilege & merged) != 0;
        }

        /// <summary>
        /// Registers the user
        /// </summary>
        /// <param name="userLoginValuesDto">The values from which the new user will be created</param>
        /// <returns>The requested actions result (true for success)</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserLoginValuesDTO userLoginValuesDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Is user exists with this email?
            var isExists = await CheckIsUserExistsAsync(userLoginValuesDto.Email);
            if (isExists)
                return ExistingUser;

            var result = await _userService.PostUserCreateAsync(userLoginValuesDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Authenticate the user
        /// </summary>
        /// <param name="userLoginValuesDto">The values from which the user will be authenticated</param>
        /// <returns>The token of the authenticated user</returns>
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateUser([FromBody] UserLoginValuesDTO userLoginValuesDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Is user exists with this email?
            var isExists = await CheckIsUserExistsAsync(userLoginValuesDto.Email);
            if (!isExists)
                return MissingUser;

            var result = await _userService.PostAutenticationAsync(userLoginValuesDto);
            if(result.AuthToken == null)
                return InvalidCredentials;
            
            return Ok(result);
        }
        
        /// <summary>
        /// Check if the given user is authenticated or not
        /// </summary>
        /// <param name="userEmailDto">The email of the given user</param>
        /// <returns>True if the user is authenticated</returns>
        [HttpGet("isauthenticated")]
        public async Task<IActionResult> GetUserIsAuthenticated([FromBody] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;

            var result = await _userService.GetUserAuthenticatedAsync(userEmailDto.Email);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the user, where the given email is tied to
        /// </summary>
        /// <param name="userEmailDto">The email of the given user</param>
        /// <returns>The users contact information</returns>
        [HttpGet("getcontact")]
        public async Task<IActionResult> GetUserContact([FromQuery] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;

            var result = await _userService.GetUserContactInformationByEmailAsync(userEmailDto.Email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get the priviliges of a given user
        /// </summary>
        /// <param name="userEmailDto">The email of the given user</param>
        /// <returns>The users privilege information</returns>
        [HttpGet("getprivilege")]
        public async Task<IActionResult> GetUserPrivilege([FromQuery] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;

            var result = await _userService.GetUserPrivilegeLevelByEmailAsync(userEmailDto.Email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get all the users reservation information
        /// </summary>
        /// <param name="userEmailDto">The email of the given user</param>
        /// <returns>All reservations tied to the user</returns>
        [Authorize]
        [HttpGet("getreservations")]
        public async Task<IActionResult> GetUserReservations([FromQuery] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userEmailDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var result = await _userService.GetUserReservationsByEmailAsync(userEmailDto.Email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get some of the users reservation information
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto containing information about the needed data</param>
        /// <returns>Selected reservations tied to the user</returns>
        [Authorize]
        [HttpGet("getreservationsselected")]
        public async Task<IActionResult> GetUserReservationsSelected([FromQuery] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var result = await _userService.GetUserSelectedReservationsByEmailAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Get all the users loan information
        /// </summary>
        /// <param name="userEmailDto">The email of the given user</param>
        /// <returns>All loans tied to the user</returns>
        [Authorize]
        [HttpGet("getloans")]
        public async Task<IActionResult> GetUserLoans([FromQuery] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userEmailDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var result = await _userService.GetUserLoansByEmailAsync(userEmailDto.Email);
            return Ok(result);
        }
        
        /// <summary>
        /// Get some of the users loan information
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto containing information about the needed data</param>
        /// <returns>Selected loans tied to the user</returns>
        [Authorize]
        [HttpGet("getloansselected")]
        public async Task<IActionResult> GetUserLoansSelected([FromQuery] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var result = await _userService.GetUserSelectedLoansByEmailAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Change the users contact information
        /// </summary>
        /// <param name="userModifyContactDto">The dto from which the contact information will be updated from</param>
        /// <returns>The result of the action (true for success)</returns>
        [Authorize]
        [HttpPut("putnewcontact")]
        public async Task<IActionResult> PutUserNewContact([FromQuery] UserModifyContactDTO userModifyContactDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userModifyContactDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userModifyContactDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userModifyContactDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.UpdateUserContactInformationAsync(userModifyContactDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Register a new email for the given user
        /// </summary>
        /// <param name="userModifyLoginDto">The dto from which the information will be updated from</param>
        /// <returns>The result of the action (true for success)</returns>
        [Authorize]
        [HttpPut("putnewemail")]
        public async Task<IActionResult> PutUserNewEmail([FromQuery] UserModifyLoginDTO userModifyLoginDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userModifyLoginDto.OldEmail);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userModifyLoginDto.OldEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userModifyLoginDto.OldEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.UpdateUserLoginAsync(userModifyLoginDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Add new reservations to the given user
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto list from which the reservations will be created</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpPatch("patchnewreservations")]
        public async Task<IActionResult> PatchUserNewReservations([FromQuery] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userCombinedRequestReservationDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.UpdateUserReservationsAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Add new loans to the given user
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto list from which the loans will be created</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpPatch("patchnewloans")]
        public async Task<IActionResult> PatchUserNewLoans([FromQuery] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userCombinedRequestLoanDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.UpdateUserLoansAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Give the specified user new permission
        /// </summary>
        /// <param name="userModifyPrivilegeDto">The dto from which the information will be updated from</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpPut("putnewpermission")]
        public async Task<IActionResult> PutUserNewPermission([FromQuery] UserModifyPrivilegeDTO userModifyPrivilegeDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userModifyPrivilegeDto.UserEmail, userModifyPrivilegeDto.RequesterEmail);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userModifyPrivilegeDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await CheckIsUserPermittedAsync(userModifyPrivilegeDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.UpdateUserPrivilegeAsync(userModifyPrivilegeDto);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a user from the database
        /// </summary>
        /// <param name="userEmailDto">The email of the user to delete</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpDelete("deleteuser")]
        public async Task<IActionResult> DeleteUser([FromQuery] UserEmailDto userEmailDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userEmailDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userEmailDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userEmailDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.RemoveUserAsync(userEmailDto.Email);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete the users given reservation information
        /// </summary>
        /// <param name="userCombinedRequestReservationDto">The dto from which the reservations will be removed by</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpDelete("deletereservations")]
        public async Task<IActionResult> DeleteUserReservations([FromQuery] UserCombinedRequestReservationDTO userCombinedRequestReservationDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestReservationDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestReservationDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await CheckIsUserPermittedAsync(userCombinedRequestReservationDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.RemoveUserReservationAsync(userCombinedRequestReservationDto.Email, userCombinedRequestReservationDto.UserReservationDto);
            return Ok(result);
        }
        
        /// <summary>
        /// Delete the users given loan information
        /// </summary>
        /// <param name="userCombinedRequestLoanDto">The dto from which the loans will be removed by</param>
        /// <returns>The result of the requested action (true for success)</returns>
        [Authorize]
        [HttpDelete("deleteloans")]
        public async Task<IActionResult> DeleteUserLoans([FromQuery] UserCombinedRequestLoanDTO userCombinedRequestLoanDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var isExists = await CheckIsUserExistsAsync(userCombinedRequestLoanDto.Email);
            if (!isExists)
                return MissingUser;
            
            var isAuthenticated = await CheckIsUserAuthenticatedAsync(userCombinedRequestLoanDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await CheckIsUserPermittedAsync(userCombinedRequestLoanDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;

            var result = await _userService.RemoveUserLoansAsync(userCombinedRequestLoanDto.Email, userCombinedRequestLoanDto.UserLoanDto);
            return Ok(result);
        }
    }
}