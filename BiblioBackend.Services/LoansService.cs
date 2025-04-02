using BiblioBackend.BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos.Loan;
using Microsoft.EntityFrameworkCore;

namespace BiblioBackend.Services
{
    public interface ILoanService
    {
        Task<List<LoanGetDTO>> GetAllLoansAsync();
        Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string UserEmail);
        Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanDto);
        Task<LoanGetDTO?> UpdateLoanAsync(int id, LoanPatchDto loanDto);
        Task<bool> DeleteLoanAsync(int id);
    }
    class LoanService : ILoanService
    {
        private readonly AppDbContext _dbContext;

        public LoanService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LoanGetDTO>> GetAllLoansAsync()
        {
            return await _dbContext.Loans
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

        public async Task<List<LoanGetDTO>?> GetLoansByUserIdAsync(string UserEmail)
        {
            var loan = await _dbContext.Loans
                .Where(l => l.UserEmail == UserEmail)
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

            return loan;
        }

        public async Task<LoanGetDTO> CreateLoanAsync(LoanPostDTO loanData)
        {
            var newLoan = new Loan
            {
                UserEmail = loanData.UserEmail,
                BookId = loanData.BookId,
                StartDate = loanData.StartTime,
                ExpectedEndDate = loanData.StartTime.AddDays(14)
            };

            _dbContext.Loans.Add(newLoan);
            await _dbContext.SaveChangesAsync();

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

        public async Task<LoanGetDTO?> UpdateLoanAsync(int id, LoanPatchDto loanDto)
        {
            var loanToUpdate = await _dbContext.Loans.FindAsync(id);

            Console.WriteLine(loanToUpdate.Id);

            Console.WriteLine(_dbContext.Loans.FirstOrDefault(l => l.Id == 1).Id);

            if (loanToUpdate == null)
            {
                return null;
            }

            Console.WriteLine("HERE");

            if (loanDto.Extensions != null) { 
                loanToUpdate.Extensions = (int)loanDto.Extensions;
            }


            if (loanDto.BookId != null)
            {
                loanToUpdate.BookId = (int)loanDto.BookId;
            }

            if (loanDto.ReturnDate != null)
            {
                loanToUpdate.ReturnDate = (DateTime)loanDto.ReturnDate;
            }

            if (loanDto.StartDate != null)
            {
                loanToUpdate.StartDate = (DateTime)loanDto.StartDate;
            }

            if (loanDto.ExpectedEndDate != null)
            {
                loanToUpdate.ExpectedEndDate = (DateTime)loanDto.ExpectedEndDate;
            }

            await _dbContext.SaveChangesAsync();

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

        public async Task<bool> DeleteLoanAsync(int id)
        {
            var loan = await _dbContext.Loans.FindAsync(id);
            if (loan == null)
            {
                return false;
            }

            _dbContext.Loans.Remove(loan);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}

