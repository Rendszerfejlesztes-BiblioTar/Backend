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
        private readonly ILoanService _loanService;
        private readonly IUserService _userService;

        public LoanController(ILoanService loanService, IUserService userService)
        {
            _loanService = loanService;
            _userService = userService;
        }

        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingLoan => NotFound("A kért kölcsön nem létezik!");
        private ObjectResult MissingLoans => NotFound("A felhasználó kölcsönjei nem léteznek!");

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var loans = await _loanService.GetAllLoansAsync();
                return Ok(loans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpGet("{userEmail}")]
        public async Task<IActionResult> GetLoansById(string userEmail)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email) || email != userEmail) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var loans = await _loanService.GetLoansByUserIdAsync(userEmail);
                if (loans == null) return MissingLoans;
                return Ok(loans);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanPostDto loanDto)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var loan = await _loanService.CreateLoanAsync(loanDto, email); // Pass email for UserEmail
                return CreatedAtAction(nameof(GetLoansById), new { userEmail = email }, loan);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanPatchDto patchDto)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var newLoan = await _loanService.UpdateLoanAsync(id, patchDto, email); // Pass email for validation/auditing
                if (newLoan == null) return MissingLoan;
                return Ok(newLoan);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian, PrivilegeLevel.Registered);
                if (!hasPermission) return NoPermission;

                var result = await _loanService.DeleteLoanAsync(id, email); // Pass email for validation/auditing
                if (!result) return MissingLoan;
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}