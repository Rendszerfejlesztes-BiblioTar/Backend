using BiblioBackend.BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Dtos.Loan;
using BiblioBackend.DataContext.Migrations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiblioBackend.Services
{
    public interface ILoanService
    {
        Task<List<LoanGetDto>> GetAllLoansAsync();
        Task<List<LoanGetDto>?> GetLoansByUserIdAsync(string UserEmail);
        Task<LoanGetDto> CreateLoanAsync(LoanPostDto loanDto);
        Task<LoanGetDto?> UpdateLoanAsync(int id, LoanModifyDto loanDto);
        Task<bool> DeleteLoanAsync(int id);
    }
    class LoanService : ILoanService
    {
        private readonly AppDbContext _dbContext;

        public LoanService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<LoanGetDto>> GetAllLoansAsync()
        {
            return await _dbContext.Loans
                .Select(loan => new LoanGetDto
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

        public async Task<List<LoanGetDto>?> GetLoansByUserIdAsync(string UserEmail)
        {
            var loan = await _dbContext.Loans
                .Where(l => l.UserEmail == UserEmail)
                .Select(loan => new LoanGetDto
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

        public async Task<LoanGetDto> CreateLoanAsync(LoanPostDto loanData)
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

            return new LoanGetDto
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

        public async Task<LoanGetDto?> UpdateLoanAsync(int id, LoanModifyDto loanDto)
        {
            var loanToUpdate = await _dbContext.Loans.FindAsync(id);

            if (loanToUpdate == null)
            {
                return null;
            }

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

            return new LoanGetDto
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

