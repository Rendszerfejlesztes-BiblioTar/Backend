using BiblioBackend.DataContext.Dtos.Book.Modify;
using BiblioBackend.DataContext.Dtos.Book.Post;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // ---
        private ObjectResult NotLoggedIn => Unauthorized("Nem vagy bejelentkezve!");
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehhez!");
        private ObjectResult MissingBook => NotFound("A kért könyv nem létezik!");
        // ---

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
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var createdBook = await _bookService.CreateBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        [Authorize]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookPatchDTO bookDto)
        {
            var updatedBook = await _bookService.UpdateBookAsync(id, bookDto);
            if (updatedBook == null) return MissingBook;
            return Ok(updatedBook);
        }

        [Authorize]
        [HttpPatch("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] BookAvailabilityPatchDTO dto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, dto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, dto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var book = await _bookService.UpdateAvailabilityAsync(dto);
            if (book == null) return MissingBook;

            return Ok(book);
        }

        [Authorize]
        [HttpPatch("quality")]
        public async Task<IActionResult> UpdateQuality([FromBody] BookQualityPatchDTO dto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, dto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermission = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, dto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!hasPermission)
                return NoPermission;

            var book = await _bookService.UpdateQualityAsync(dto);
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
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return MissingBook;
            return NoContent();
        }
    }
}