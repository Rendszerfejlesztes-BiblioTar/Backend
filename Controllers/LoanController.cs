using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.DataContext.Dtos.Loan;
using BiblioBackend.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _service;
        private readonly IUserService _userService;

        public LoanController(ILoanService loanService, IUserService userService)
        {
            _service = loanService;
            _userService = userService;
        }

        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        private ObjectResult MissingLoan => NotFound("A kért kölcsön nem létezik!");

        /// <summary>
        /// Get all loans in database
        /// </summary>
        /// <returns>A list of every single loan</returns>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans([FromBody] LoanGetAllDto loanGetAllDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != loanGetAllDto.Email)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var loans = await _service.GetAllLoansAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Get every loan tied to an email
        /// </summary>
        /// <param name="userEmail">The email to get the loans from</param>
        /// <returns>The list of loans tied to that email</returns>
        [Authorize]
        [HttpGet("{userEmail}")]
        public async Task<IActionResult> GetLoansById(string userEmail)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != userEmail)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission)
                return NoPermission;

            var loans = await _service.GetLoansByUserIdAsync(userEmail);
            return Ok(loans);
        }

        /// <summary>
        /// Create a loan for the library
        /// </summary>
        /// <param name="loanDto">The loan dto to create a new loan from</param>
        /// <returns>The created loan</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanPostDTO loanDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != loanDto.UserEmail)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission)
                return NoPermission;

            var loan = await _service.CreateLoanAsync(loanDto);
            return CreatedAtAction(nameof(GetLoansById), new { userEmail = loanDto.UserEmail }, loan);
        }

        /// <summary>
        /// Update an existing loan
        /// </summary>
        /// <param name="id">The id of the loan</param>
        /// <param name="patchDto">The new loan information</param>
        /// <returns>The new loan dto</returns>
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanPatchDto patchDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != patchDto.UserEmail)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission)
                return NoPermission;

            var newLoan = await _service.UpdateLoanAsync(id, patchDto);
            if (newLoan == null)
                return MissingLoan;

            return Ok(newLoan);
        }

        /// <summary>
        /// Delete a loan
        /// </summary>
        /// <param name="id">The id of the loan to delete</param>
        /// <param name="loanDeleteDto">The dto to do the deletion from</param>
        /// <returns>True if deleted</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id, [FromBody] LoanDeleteDto loanDeleteDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != loanDeleteDto.Email)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission)
                return NoPermission;

            var result = await _service.DeleteLoanAsync(id);
            if (!result)
                return MissingLoan;

            return Ok(result);
        }
    }
}