using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Dtos.Loan;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BiblioBackend.BiblioBackend.DataContext.Entities;

namespace BiblioBackend.Services
{
    public interface ILoanService
    {
        Task<List<LoanGetDTO>> GetAllLoansAsync();
        Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string userEmail);
        Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanDto, string userEmail);
        Task<LoanGetDTO> UpdateLoanAsync(int id, LoanPatchDto loanDto, string userEmail);
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
            return await _context.Loans
                .Select(loan => new LoanGetDTO
                {
                    Id = loan.Id,
                    UserEmail = loan.UserEmail,
                    BookId = loan.BookId,
                    Extensions = loan.Extensions,
                    StartDate = loan.StartDate,
                    ExpectedEndDate = loan.ExpectedEndDate,
                    ReturnDate = loan.ReturnDate
                })
                .ToListAsync();
        }

        public async Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string userEmail)
        {
            _logger.LogInformation("Retrieving loans for user {Email}", userEmail);
            var loans = await _context.Loans
                .Where(l => l.UserEmail == userEmail)
                .Select(loan => new LoanGetDTO
                {
                    Id = loan.Id,
                    UserEmail = loan.UserEmail,
                    BookId = loan.BookId,
                    Extensions = loan.Extensions,
                    StartDate = loan.StartDate,
                    ExpectedEndDate = loan.ExpectedEndDate,
                    ReturnDate = loan.ReturnDate
                })
                .ToListAsync();

            return loans.Any() ? loans : null;
        }

        public async Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanDto, string userEmail)
        {
            _logger.LogInformation("Creating loan for user {Email}", userEmail);

            var book = await _context.Books.FindAsync(loanDto.BookId);
            if (book == null)
            {
                _logger.LogWarning("Cannot create loan: Book {BookId} not found", loanDto.BookId);
                throw new InvalidOperationException("Book not found.");
            }
            if (!book.IsAvailable)
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

        public async Task<LoanGetDTO> UpdateLoanAsync(int id, LoanPatchDto loanDto, string userEmail)
        {
            _logger.LogInformation("Updating loan {Id} by {Email}", id, userEmail);
            var loanToUpdate = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (loanToUpdate == null)
            {
                _logger.LogWarning("Loan {Id} not found", id);
                throw new KeyNotFoundException($"Loan with ID {id} not found.");
            }

            var isAdminOrLibrarian = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!isAdminOrLibrarian && loanToUpdate.UserEmail != userEmail)
            {
                _logger.LogWarning("User {Email} cannot update loan {Id} belonging to another user", userEmail, id);
                throw new UnauthorizedAccessException("Cannot update another user's loan.");
            }

            if (loanDto.BookId.HasValue && loanDto.BookId != loanToUpdate.BookId)
            {
                var newBook = await _context.Books.FindAsync(loanDto.BookId);
                if (newBook == null)
                {
                    _logger.LogWarning("Cannot update loan: Book {BookId} not found", loanDto.BookId);
                    throw new InvalidOperationException("New book not found.");
                }
                if (!newBook.IsAvailable)
                {
                    _logger.LogWarning("Cannot update loan: Book {BookId} is not available", loanDto.BookId);
                    throw new InvalidOperationException("New book is not available.");
                }

                if (loanToUpdate.ReturnDate == null)
                {
                    loanToUpdate.Book.IsAvailable = true;
                }
                loanToUpdate.BookId = loanDto.BookId.Value;
                newBook.IsAvailable = false;
            }

            loanToUpdate.Extensions = loanDto.Extensions ?? loanToUpdate.Extensions;
            loanToUpdate.StartDate = loanDto.StartDate ?? loanToUpdate.StartDate;
            loanToUpdate.ExpectedEndDate = loanDto.ExpectedEndDate ?? loanToUpdate.ExpectedEndDate;
            loanToUpdate.ReturnDate = loanDto.ReturnDate ?? loanToUpdate.ReturnDate;

            if (loanDto.ReturnDate != null && loanToUpdate.Book != null && loanToUpdate.ReturnDate == null)
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
            var loan = await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);

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

            if (loan.ReturnDate == null && loan.Book != null)
            {
                loan.Book.IsAvailable = true;
            }

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Loan {Id} deleted by {Email}", id, userEmail);
            return true;
        }
    }
}