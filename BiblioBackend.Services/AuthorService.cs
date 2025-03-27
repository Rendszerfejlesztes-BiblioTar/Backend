using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiblioBackend.Dtos.Author;

namespace BiblioBackend.Services
{
    public interface IAuthorService
    {
        Task<List<AuthorGetDto>> GetAllAuthorsAsync();
        Task<AuthorGetDto?> GetAuthorByIdAsync(int id);
        Task<AuthorGetDto> CreateAuthorAsync(AuthorModifyDto authorDto);
        Task<AuthorGetDto?> UpdateAuthorAsync(int id, AuthorModifyDto authorDto);
        Task<bool> DeleteAuthorAsync(int id);
    }
    class AuthorService : IAuthorService
    {
        private readonly AppDbContext _dbContext;

        public AuthorService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AuthorGetDto>> GetAllAuthorsAsync()
        {
            return await _dbContext.Authors
                .Select(author => new AuthorGetDto
                {
                    Id = author.Id,
                    Name = author.Name
                })
                .ToListAsync();
        }

        public async Task<AuthorGetDto?> GetAuthorByIdAsync(int id)
        {
            var author = await _dbContext.Authors
                .Where(a => a.Id == id)
                .Select(a => new AuthorGetDto
                {
                    Id = a.Id,
                    Name = a.Name
                })
                .FirstOrDefaultAsync();

            return author;
        }

        public async Task<AuthorGetDto> CreateAuthorAsync(AuthorModifyDto authorDto)
        {
            var author = new Author
            {
                Name = authorDto.Name
            };

            _dbContext.Authors.Add(author);
            await _dbContext.SaveChangesAsync();

            return new AuthorGetDto
            {
                Id = author.Id,
                Name = author.Name
            };
        }

        public async Task<AuthorGetDto?> UpdateAuthorAsync(int id, AuthorModifyDto authorDto)
        {
            var authorToUpdate = await _dbContext.Authors.FindAsync(id);

            if (authorToUpdate == null)
            {
                return null;
            }

            authorToUpdate.Name = authorDto.Name;

            await _dbContext.SaveChangesAsync();

            return new AuthorGetDto
            {
                Id = authorToUpdate.Id,
                Name = authorToUpdate.Name
            };
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            var author = await _dbContext.Authors.FindAsync(id);
            if (author == null)
            {
                return false;
            }

            _dbContext.Authors.Remove(author);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
