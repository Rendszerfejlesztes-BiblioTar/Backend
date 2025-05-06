using BiblioBackend.BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Dtos.User.Post;
using BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    [EnableRateLimiting("auth")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        public class ApiResponse<T>
        {
            public T? Data { get; set; }
            public string? Error { get; set; }
            public bool Success => string.IsNullOrEmpty(Error);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ApiResponse<UserDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<UserDto>))]
        public async Task<IActionResult> RegisterUser([FromBody] UserLoginValuesDto userDto)
        {
            try
            {
                var result = await _userService.RegisterUserAsync(userDto);
                return CreatedAtAction(nameof(GetUser), new { email = result.Email }, new ApiResponse<UserDto> { Data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<UserDto> { Error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserLoginDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<UserLoginDto>))]
        public async Task<IActionResult> AuthenticateUser([FromBody] UserLoginValuesDto userDto)
        {
            try
            {
                var result = await _userService.AuthenticateUserAsync(userDto);
                return Ok(new ApiResponse<UserLoginDto> { Data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new ApiResponse<UserLoginDto> { Error = ex.Message });
            }
        }

        [HttpPost("create-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateAdmin([FromBody] UserLoginValuesDto userDto)
        {
            if (string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
            {
                return BadRequest(new { data = new object(), error = "Email and password are required.", success = false });
            }

            if (await _userService.GetUserIsExistsAsync(userDto.Email))
            {
                return Conflict(new { data = new object(), error = "User already exists.", success = false });
            }

            var adminUser = new UserLoginValuesDto
            {
                Email = userDto.Email,
                Password = userDto.Password
            };

            var user = await _userService.RegisterUserAsync(adminUser);
            await _userService.UpdateUserPrivilegeAsync(new UserModifyPrivilegeDto
            {
                UserEmail = user.Email,
                RequesterEmail = user.Email, 
                NewPrivilege = PrivilegeLevel.Admin
            });

            return Ok(new { data = user, error = "", success = true });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserLoginDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<UserLoginDto>))]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _userService.RefreshTokenAsync(refreshTokenDto);
                return Ok(new ApiResponse<UserLoginDto> { Data = result });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new ApiResponse<UserLoginDto> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "UserAccess")]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> Logout()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<bool> { Error = "User is not authenticated." });

            try
            {
                var result = await _userService.RevokeTokenAsync(email);
                return Ok(new ApiResponse<bool> { Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "UserAccess")]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<UserDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<UserDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<UserDto>))]
        public async Task<IActionResult> GetUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new ApiResponse<UserDto> { Error = "User is not authenticated." });

            try
            {
                var result = await _userService.GetUserAsync(email);
                return Ok(new ApiResponse<UserDto> { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<UserDto> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "UserAccess")]
        [HttpPut("contact")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> UpdateUserContact([FromBody] UserModifyContactDto userDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != userDto.Email)
                return Unauthorized(new ApiResponse<bool> { Error = "User is not authorized to update this contact." });

            try
            {
                var result = await _userService.UpdateUserContactAsync(userDto);
                return Ok(new ApiResponse<bool> { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<bool> { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "UserAccess")]
        [HttpPut("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> UpdateUserLogin([FromBody] UserModifyLoginDto userDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != userDto.OldEmail)
                return Unauthorized(new ApiResponse<bool> { Error = "User is not authorized to update this login." });

            try
            {
                var result = await _userService.UpdateUserLoginAsync(userDto);
                return Ok(new ApiResponse<bool> { Data = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<bool> { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminAccess")]
        [HttpPut("privilege")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> UpdateUserPrivilege([FromBody] UserModifyPrivilegeDto userDto)
        {
            var requesterEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(requesterEmail))
                return Unauthorized(new ApiResponse<bool> { Error = "Requester is not authenticated." });

            userDto.RequesterEmail = requesterEmail;

            try
            {
                var result = await _userService.UpdateUserPrivilegeAsync(userDto);
                if (!result)
                    return NotFound(new ApiResponse<bool> { Error = "User or requester not found." });
                return Ok(new ApiResponse<bool> { Data = result });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }

        [Authorize(Policy = "AdminAccess")]
        [HttpDelete("{email}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> DeleteUser(string email)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(email);
                if (!result)
                    return NotFound(new ApiResponse<bool> { Error = $"User with email {email} not found." });
                return Ok(new ApiResponse<bool> { Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }
        
        [Authorize(Policy = "AdminAccess")]
        [HttpGet("get-all-users")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<UserDto>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiResponse<bool>))]
        public async Task<IActionResult> GetAllRegisteredUsers()
        {
            try
            {
                var result = await _userService.GetAllRegisteredUsersAsync();
                if (result.Count <= 0)
                    return NotFound(new ApiResponse<bool> { Error = $"No registered users have been found!" });
                return Ok(new ApiResponse<List<UserDto>> { Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<bool> { Error = ex.Message });
            }
        }
        
        [AllowAnonymous]
        [HttpPost("create-default-admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DefaultAdminDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<DefaultAdminDto>))]
        public async Task<IActionResult> CreateDefaultAdminUser()
        {
            try
            {
                var result = await _userService.CreateDefaultAdmin();
                return Ok(new ApiResponse<DefaultAdminDto> { Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<DefaultAdminDto> { Error = ex.Message });
            }
        }
    }
}