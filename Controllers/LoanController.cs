using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.DataContext.Dtos.Loan;
using BiblioBackend.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;

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
        
        // ---
        
        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        private ObjectResult MissingLoan => NotFound("A kért kölcsön nem létezik!");
        
        // ---

        /// <summary>
        /// Get all loans in database
        /// </summary>
        /// <returns>A list of every single loan</returns>
        [Authorize] //Since this gives back all the loans in the db currently, basic security measure is that this is only permitted for librarian and up
        [HttpGet]
        public async Task<IActionResult> GetAllLoans([FromBody] LoanGetAllDto loanGetAllDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, loanGetAllDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, loanGetAllDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var Loans = await _service.GetAllLoansAsync();
            return Ok(Loans);
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
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, userEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var Loans = await _service.GetLoansByUserIdAsync(userEmail);
            return Ok(Loans);
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
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, loanDto.UserEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, loanDto.UserEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var Loan = await _service.CreateLoanAsync(loanDto);
            return CreatedAtAction(nameof(CreateLoan), loanDto);
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
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, patchDto.UserEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, patchDto.UserEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var newLoan = await _service.UpdateLoanAsync(id, patchDto);

            if (newLoan == null)
                return MissingLoan;

            return Ok(newLoan);
        }

        /// <summary>
        /// Delete a loan
        /// </summary>
        /// <param name="loanDeleteDto">The dto to do the deletion from</param>
        /// <returns>True if deleted</returns>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteLoan([FromBody] LoanDeleteDto loanDeleteDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, loanDeleteDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, loanDeleteDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _service.DeleteLoanAsync(loanDeleteDto.Id);

            if (!result)
                return MissingLoan;

            return Ok(result);
        }
    }
}
