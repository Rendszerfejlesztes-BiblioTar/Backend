using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiblioBackend.DataContext.Dtos.Category;


namespace BiblioBackend.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryGetDto>> GetAllCategoriesAsync();
        Task<CategoryGetDto?> GetCategoryByIdAsync(int id);
        Task<CategoryGetDto> CreateCategoryAsync(CategoryModifyDto categoryDto);
        Task<CategoryGetDto?> UpdateCategoryAsync(int id, CategoryModifyDto categoryDto);
        Task<bool> DeleteCategoryAsync(int id);
    }

    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _dbContext;

        public CategoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CategoryGetDto>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .Select(category => new CategoryGetDto
                {
                    Id = category.Id,
                    Name = category.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryGetDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _dbContext.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryGetDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .FirstOrDefaultAsync();

            return category;
        }

        public async Task<CategoryGetDto> CreateCategoryAsync(CategoryModifyDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            return new CategoryGetDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryGetDto?> UpdateCategoryAsync(int id, CategoryModifyDto categoryDto)
        {
            var categoryToUpdate = await _dbContext.Categories.FindAsync(id);

            if (categoryToUpdate == null)
            {
                return null;
            }

            categoryToUpdate.Name = categoryDto.Name;

            await _dbContext.SaveChangesAsync();

            return new CategoryGetDto
            {
                Id = categoryToUpdate.Id,
                Name = categoryToUpdate.Name
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
