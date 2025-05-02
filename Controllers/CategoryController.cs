using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.DataContext.Dtos.Category;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;

        public CategoryController(ICategoryService categoryService, IUserService userService)
        {
            _categoryService = categoryService;
            _userService = userService;
        }

        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingCategory => NotFound("A kért kategória nem létezik!");

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
            if (category == null) return MissingCategory;
            return Ok(category);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryModifyDto categoryDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var newCategory = await _categoryService.CreateCategoryAsync(categoryDto, email); // Pass email for auditing
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryModifyDto categoryDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var newCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto, email); // Pass email for auditing
            if (newCategory == null) return MissingCategory;
            return Ok(newCategory);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var result = await _categoryService.DeleteCategoryAsync(id, email); // Pass email for auditing
            if (!result) return MissingCategory;
            return Ok(result);
        }
    }
}