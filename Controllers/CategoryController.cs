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
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        private ObjectResult MissingCategory => NotFound("A kért kategória nem létezik!");

        /// <summary>
        /// List of all valid book categories
        /// </summary>
        /// <returns>A dto list containing every book category</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Get a specific category based on its Id
        /// </summary>
        /// <param name="id">The id to look for</param>
        /// <returns>The category result</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return MissingCategory;

            return Ok(category);
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="categoryDto">The dto to create from</param>
        /// <returns>The created category dto</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryModifyDto categoryDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != categoryDto.Email)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var newCategory = await _categoryService.CreateCategoryAsync(categoryDto);
            return CreatedAtAction(nameof(GetCategoryById), new { id = newCategory.Id }, newCategory);
        }

        /// <summary>
        /// Update a given category
        /// </summary>
        /// <param name="id">The category id</param>
        /// <param name="categoryDto">The dto to update from</param>
        /// <returns>The updated category</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryModifyDto categoryDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != categoryDto.Email)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var newCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto);
            if (newCategory == null)
                return MissingCategory;

            return Ok(newCategory);
        }

        /// <summary>
        /// Delete a given category
        /// </summary>
        /// <param name="id">The id of the category to delete</param>
        /// <param name="categoryDeleteDto">The dto to do the deletion from</param>
        /// <returns>True if success</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id, [FromBody] CategoryDeleteDto categoryDeleteDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email) || email != categoryDeleteDto.Email)
                return NotLoggedIn;

            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result)
                return MissingCategory;

            return Ok(result);
        }
    }
}