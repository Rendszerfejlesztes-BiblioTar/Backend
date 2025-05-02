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
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingLoan => NotFound("A kért kölcsön nem létezik!");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var loans = await _service.GetAllLoansAsync();
            return Ok(loans);
        }

        [Authorize]
        [HttpGet("{userEmail}")]
        public async Task<IActionResult> GetLoansById(string userEmail)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != userEmail) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission) return NoPermission;

            var loans = await _service.GetLoansByUserIdAsync(userEmail);
            return Ok(loans);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanPostDTO loanDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission) return NoPermission;

            var loan = await _service.CreateLoanAsync(loanDto, email); // Pass email for UserEmail
            return CreatedAtAction(nameof(GetLoansById), new { userEmail = email }, loan);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanPatchDto patchDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission) return NoPermission;

            var newLoan = await _service.UpdateLoanAsync(id, patchDto, email); // Pass email for validation/auditing
            if (newLoan == null) return MissingLoan;
            return Ok(newLoan);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
            if (!hasPermission) return NoPermission;

            var result = await _service.DeleteLoanAsync(id, email); // Pass email for validation/auditing
            if (!result) return MissingLoan;
            return Ok(result);
        }
    }
}