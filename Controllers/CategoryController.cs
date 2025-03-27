using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.DataContext.Dtos.Category;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryModifyDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newCategory = await _categoryService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryModifyDto categoryDto)
        {
            var newCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto);

            if (newCategory == null)
            {
                return NotFound();
            }

            return Ok(newCategory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}

