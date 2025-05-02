using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using BiblioBackend.DataContext.Dtos.Category;
using Microsoft.Extensions.Logging;

namespace BiblioBackend.Services
{
    public interface ICategoryService
    {
        Task<List<CategoryGetDto>> GetAllCategoriesAsync();
        Task<CategoryGetDto?> GetCategoryByIdAsync(int id);
        Task<CategoryGetDto> CreateCategoryAsync(CategoryModifyDto categoryDto, string requesterEmail);
        Task<CategoryGetDto?> UpdateCategoryAsync(int id, CategoryModifyDto categoryDto, string requesterEmail);
        Task<bool> DeleteCategoryAsync(int id, string requesterEmail);
    }

    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext dbContext, ILogger<CategoryService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<CategoryGetDto>> GetAllCategoriesAsync()
        {
            _logger.LogInformation("Retrieving all categories");
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
            _logger.LogInformation("Retrieving category {Id}", id);
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

        public async Task<CategoryGetDto> CreateCategoryAsync(CategoryModifyDto categoryDto, string requesterEmail)
        {
            _logger.LogInformation("Creating category by {Email}", requesterEmail);

            // Validate unique name
            if (await _dbContext.Categories.AnyAsync(c => c.Name == categoryDto.Name))
            {
                _logger.LogWarning("Category {Name} already exists", categoryDto.Name);
                throw new InvalidOperationException("Category name already exists.");
            }

            var category = new Category
            {
                Name = categoryDto.Name
            };

            _dbContext.Categories.Add(category);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Category {Name} created by {Email}", categoryDto.Name, requesterEmail);

            return new CategoryGetDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<CategoryGetDto?> UpdateCategoryAsync(int id, CategoryModifyDto categoryDto, string requesterEmail)
        {
            _logger.LogInformation("Updating category {Id} by {Email}", id, requesterEmail);
            var categoryToUpdate = await _dbContext.Categories.FindAsync(id);

            if (categoryToUpdate == null)
            {
                _logger.LogWarning("Category {Id} not found", id);
                return null;
            }

            // Validate unique name
            if (categoryDto.Name != categoryToUpdate.Name && await _dbContext.Categories.AnyAsync(c => c.Name == categoryDto.Name))
            {
                _logger.LogWarning("Category {Name} already exists", categoryDto.Name);
                throw new InvalidOperationException("Category name already exists.");
            }

            categoryToUpdate.Name = categoryDto.Name;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Category {Id} updated by {Email}", id, requesterEmail);

            return new CategoryGetDto
            {
                Id = categoryToUpdate.Id,
                Name = categoryToUpdate.Name
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id, string requesterEmail)
        {
            _logger.LogInformation("Deleting category {Id} by {Email}", id, requesterEmail);
            var category = await _dbContext.Categories.FindAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category {Id} not found", id);
                return false;
            }

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Category {Id} deleted by {Email}", id, requesterEmail);
            return true;
        }
    }
}