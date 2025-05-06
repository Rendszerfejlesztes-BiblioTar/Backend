using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using BiblioBackend.DataContext.Dtos.Loan;
using BiblioBackend.BiblioBackend.DataContext.Entities;
using Microsoft.Extensions.Logging;

namespace BiblioBackend.Services
{
    public interface ILoanService
    {
        Task<List<LoanGetDTO>> GetAllLoansAsync();
        Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string userEmail);
        Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanDto, string userEmail);
        Task<LoanGetDTO?> UpdateLoanAsync(int id, LoanPatchDto loanDto, string userEmail);
        Task<bool> DeleteLoanAsync(int id, string userEmail);
    }

    public class LoanService : ILoanService 
    {
        private readonly AppDbContext _context; 
        private readonly ILogger<LoanService> _logger;
        private readonly IUserService _userService; // Added for permission checks
        
        public LoanService(AppDbContext context, ILogger<LoanService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        public async Task<List<LoanGetDTO>> GetAllLoansAsync()
        {
            _logger.LogInformation("Retrieving all loans");
            var loanList = await _context.Loans.ToListAsync();
            
            return loanList.ConvertAll(loan => new LoanGetDTO()
                {
                    Id = loan.Id,
                    UserEmail = loan.UserEmail,
                    BookId = loan.BookId,
                    Extensions = loan.Extensions,
                    StartDate = loan.StartDate,
                    ExpectedEndDate = loan.ExpectedEndDate,
                    ReturnDate = loan.ReturnDate
                });
        }

        public async Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string userEmail)
        {
            _logger.LogInformation("Retrieving loans for user {Email}", userEmail);
            var loans = _context.Loans.Where(l => l.UserEmail == userEmail);
            if(!loans.Any())
                return null;

            var selectedLoans = (await loans.ToListAsync()).ConvertAll(loan => new LoanGetDTO
            {
                Id = loan.Id,
                UserEmail = loan.UserEmail,
                BookId = loan.BookId,
                Extensions = loan.Extensions,
                StartDate = loan.StartDate,
                ExpectedEndDate = loan.ExpectedEndDate,
                ReturnDate = loan.ReturnDate
            });

            return selectedLoans;
        }

        public async Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanDto, string userEmail)
        {
            _logger.LogInformation("Creating loan for user {Email}", userEmail);

            // Validate book availability
            var book = await _context.Books.FindAsync(loanDto.BookId);
            if (book == null || !book.IsAvailable)
            {
                _logger.LogWarning("Cannot create loan: Book {BookId} is not available", loanDto.BookId);
                throw new InvalidOperationException("Book is not available.");
            }

            var newLoan = new Loan
            {
                UserEmail = userEmail,
                BookId = loanDto.BookId,
                StartDate = loanDto.StartTime,
                ExpectedEndDate = loanDto.StartTime.AddDays(14)
            };

            _context.Loans.Add(newLoan);
            book.IsAvailable = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan created for user {Email}, book {BookId}", userEmail, loanDto.BookId);

            return new LoanGetDTO
            {
                Id = newLoan.Id,
                UserEmail = newLoan.UserEmail,
                BookId = newLoan.BookId,
                Extensions = newLoan.Extensions,
                StartDate = newLoan.StartDate,
                ExpectedEndDate = newLoan.ExpectedEndDate,
                ReturnDate = newLoan.ReturnDate
            };
        }

        public async Task<LoanGetDTO?> UpdateLoanAsync(int id, LoanPatchDto loanDto, string userEmail)
        {
            _logger.LogInformation("Updating loan {Id} by {Email}", id, userEmail);
            var loanToUpdate = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loanToUpdate == null)
            {
                _logger.LogWarning("Loan {Id} not found", id);
                return null;
            }

            // Validate user permission
            var isAdminOrLibrarian = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!isAdminOrLibrarian && loanToUpdate.UserEmail != userEmail)
            {
                _logger.LogWarning("User {Email} cannot update loan {Id} belonging to another user", userEmail, id);
                throw new UnauthorizedAccessException("Cannot update another user's loan.");
            }

            loanToUpdate.Extensions = loanDto.Extensions ?? loanToUpdate.Extensions;
            loanToUpdate.BookId = loanDto.BookId ?? loanToUpdate.BookId;
            loanToUpdate.StartDate = loanDto.StartDate ?? loanToUpdate.StartDate;
            loanToUpdate.ExpectedEndDate = loanDto.ExpectedEndDate ?? loanToUpdate.ExpectedEndDate;
            loanToUpdate.ReturnDate = loanDto.ReturnDate ?? loanToUpdate.ReturnDate;

            if (loanDto.ReturnDate != null && loanToUpdate.Book != null)
            {
                loanToUpdate.Book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan {Id} updated by {Email}", id, userEmail);

            return new LoanGetDTO
            {
                Id = loanToUpdate.Id,
                UserEmail = loanToUpdate.UserEmail,
                BookId = loanToUpdate.BookId,
                Extensions = loanToUpdate.Extensions,
                StartDate = loanToUpdate.StartDate,
                ExpectedEndDate = loanToUpdate.ExpectedEndDate,
                ReturnDate = loanToUpdate.ReturnDate
            };
        }

        public async Task<bool> DeleteLoanAsync(int id, string userEmail)
        {
            _logger.LogInformation("Deleting loan {Id} by {Email}", id, userEmail);
            var loan = await _context.Loans.FindAsync(id);
            if (loan == null)
            {
                _logger.LogWarning("Loan {Id} not found", id);
                return false;
            }

            var isAdminOrLibrarian = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!isAdminOrLibrarian && loan.UserEmail != userEmail)
            {
                _logger.LogWarning("User {Email} cannot delete loan {Id} belonging to another user", userEmail, id);
                throw new UnauthorizedAccessException("Cannot delete another user's loan.");
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan {Id} deleted by {Email}", id, userEmail);
            return true;
        }
    }
}