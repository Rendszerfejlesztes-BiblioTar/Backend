using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Dtos.Loan;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _service;

        public LoanController(ILoanService loanService)
        {
            _service = loanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLoans()
        {
            var Loans = await _service.GetAllLoansAsync();
            return Ok(Loans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoansById(string id)
        {
            var Loans = await _service.GetLoansByUserIdAsync(id);
            return Ok(Loans);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] LoanPostDto loan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Loan = await _service.CreateLoanAsync(loan);
            return CreatedAtAction(nameof(CreateLoan), loan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLoan(int id, [FromBody] LoanModifyDto modifyDto)
        {
            var newLoan = await _service.UpdateLoanAsync(id, modifyDto);

            if (newLoan != null)
            {
                return NotFound();
            }

            return Ok(newLoan);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLoan(int id)
        {
            var result = await _service.DeleteLoanAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return Ok();
        }
    }
}
