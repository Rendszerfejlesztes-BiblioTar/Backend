using BiblioBackend.DataContext.Dtos.Book.Modify;
using BiblioBackend.DataContext.Dtos.Book.Post;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;

        public BookController(IBookService bookService, IUserService userService)
        {
            _bookService = bookService;
            _userService = userService;
        }

        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingBook => NotFound("A kért könyv nem létezik!");

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return MissingBook;
            return Ok(book);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookPostDTO bookDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var createdBook = await _bookService.CreateBookAsync(bookDto, email); // Pass email for auditing
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookPatchDTO bookDto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var updatedBook = await _bookService.UpdateBookAsync(id, bookDto, email); // Pass email for auditing
            if (updatedBook == null) return MissingBook;
            return Ok(updatedBook);
        }

        [Authorize]
        [HttpPatch("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] BookAvailabilityPatchDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var book = await _bookService.UpdateAvailabilityAsync(dto, email); // Pass email for auditing
            if (book == null) return MissingBook;
            return Ok(book);
        }

        [Authorize]
        [HttpPatch("quality")]
        public async Task<IActionResult> UpdateQuality([FromBody] BookQualityPatchDTO dto)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var book = await _bookService.UpdateQualityAsync(dto, email); // Pass email for auditing
            if (book == null) return MissingBook;
            return Ok(book);
        }

        [HttpGet("search/title")]
        public async Task<IActionResult> SearchBooksByName(string title)
        {
            var filtered = await _bookService.SearchBooksByNameAsync(title);
            if (filtered.Count == 0) return MissingBook;
            return Ok(filtered);
        }

        [HttpGet("search/author")]
        public async Task<IActionResult> SearchBooksByAuthor(string author)
        {
            var filtered = await _bookService.SearchBooksByAuthorAsync(author);
            if (filtered.Count == 0) return MissingBook;
            return Ok(filtered);
        }

        [HttpGet("search/category")]
        public async Task<IActionResult> SearchBooksByCategory(string category)
        {
            var filtered = await _bookService.SearchBooksByCategoryAsync(category);
            if (filtered.Count == 0) return MissingBook;
            return Ok(filtered);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission) return NoPermission;

            var result = await _bookService.DeleteBookAsync(id, email); // Pass email for auditing
            if (!result) return MissingBook;
            return NoContent();
        }
    }
}