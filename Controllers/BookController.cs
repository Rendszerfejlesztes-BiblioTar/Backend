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
            try
            {
                var books = await _bookService.GetAllBooksAsync();
                return Ok(books);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            try
            {
                var book = await _bookService.GetBookByIdAsync(id);
                if (book == null) return MissingBook;
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookPostDto bookDto)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var createdBook = await _bookService.CreateBookAsync(bookDto, email); // Pass email for auditing
                return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookPatchDto bookDto)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var updatedBook = await _bookService.UpdateBookAsync(id, bookDto, email); // Pass email for auditing
                if (updatedBook == null) return MissingBook;
                return Ok(updatedBook);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPatch("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] BookAvailabilityPatchDto dto)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var book = await _bookService.UpdateAvailabilityAsync(dto, email); // Pass email for auditing
                if (book == null) return MissingBook;
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpPatch("quality")]
        public async Task<IActionResult> UpdateQuality([FromBody] BookQualityPatchDto dto)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var book = await _bookService.UpdateQualityAsync(dto, email); // Pass email for auditing
                if (book == null) return MissingBook;
                return Ok(book);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("search/title")]
        public async Task<IActionResult> SearchBooksByName(string title)
        {
            try
            {
                var filtered = await _bookService.SearchBooksByNameAsync(title);
                if (filtered.Count == 0) return MissingBook;
                return Ok(filtered);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("search/author")]
        public async Task<IActionResult> SearchBooksByAuthor(string author)
        {
            try
            {
                var filtered = await _bookService.SearchBooksByAuthorAsync(author);
                if (filtered.Count == 0) return MissingBook;
                return Ok(filtered);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("search/category")]
        public async Task<IActionResult> SearchBooksByCategory(string category)
        {
            try
            {
                var filtered = await _bookService.SearchBooksByCategoryAsync(category);
                if (filtered.Count == 0) return MissingBook;
                return Ok(filtered);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(email)) return NotLoggedIn;

                var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
                if (!hasPermission) return NoPermission;

                var result = await _bookService.DeleteBookAsync(id, email); // Pass email for auditing
                if (!result) return MissingBook;
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}