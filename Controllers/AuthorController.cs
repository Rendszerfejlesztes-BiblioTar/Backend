using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using BiblioBackend.DataContext.Dtos.Author;
using Microsoft.AspNetCore.Authorization;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        private readonly IUserService _userService;

        public AuthorController(IAuthorService authorService, IUserService userService)
        {
            _authorService = authorService;
            _userService = userService;
        }
        // ---
        
        private ObjectResult NoAuthor => NotFound("A kért író nem található!");
        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        
        // ---

        /// <summary>
        /// Get all authors in the database
        /// </summary>
        /// <returns>Every single author</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }

        /// <summary>
        /// Get a specific author from the database
        /// </summary>
        /// <param name="id">The id of the author</param>
        /// <returns>The requested author</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
                return NoAuthor;

            return Ok(author);
        }

        /// <summary>
        /// Creates a new author
        /// </summary>
        /// <param name="authorDto">The dto to create from</param>
        /// <returns>The result of the requested action</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAuthor([FromBody] AuthorModifyDto authorDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, authorDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            // only librarian or admin can create new authors
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, authorDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var newAuthor = await _authorService.CreateAuthorAsync(authorDto);
            return CreatedAtAction(nameof(GetAuthorById), new { id = newAuthor.Id }, newAuthor);
        }

        /// <summary>
        /// Update the authors value
        /// </summary>
        /// <param name="id">The id of the author to update</param>
        /// <param name="authorDto">The dto containing extra data regarding the modification</param>
        /// <returns>True if modified</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuthor(int id, [FromBody] AuthorModifyDto authorDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, authorDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, authorDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var newAuthor = await _authorService.UpdateAuthorAsync(id, authorDto);

            if (newAuthor == null)
                return NoAuthor;

            return Ok(newAuthor);
        }

        /// <summary>
        /// Delete the specified author
        /// </summary>
        /// <param name="id">THe id of the author to delete</param>
        /// <param name="authorPermissionRestrictedActionDto">dto containing extra information</param>
        /// <returns>True if deleted</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthor(int id, [FromBody] AuthorPermissionRestrictedActionDto authorPermissionRestrictedActionDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, authorPermissionRestrictedActionDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;
            
            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, authorPermissionRestrictedActionDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _authorService.DeleteAuthorAsync(id);
            if (!result)
                return NoAuthor;

            return Ok(result);
        }
    }
}
