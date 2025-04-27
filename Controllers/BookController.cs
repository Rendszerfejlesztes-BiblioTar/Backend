using BiblioBackend.DataContext.Dtos.Book.Modify;
using BiblioBackend.DataContext.Dtos.Book.Post;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

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
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpGet("search/title={title}")]
        public async Task<IActionResult> SearchBooksByName(string title)
        {
            var filtered = await _bookService.SearchBooksByNameAsync(title);
            if (filtered.Count == 0)
            {
                return NotFound("A megadott címmel nem létezik könyv!");
            }
            return Ok(filtered);
        }

        [HttpGet("search/author={author}")]
        public async Task<IActionResult> SearchBooksByAuthor(string author)
        {
            var filtered = await _bookService.SearchBooksByAuthorAsync(author);
            if (filtered.Count == 0)
            {
                return NotFound("A megadott szertõvel nem létezik könyv!");
            }
            return Ok(filtered);
        }

        [HttpGet("search/category={category}")]
        public async Task<IActionResult> SearchBooksByCategory(string category)
        {
            var filtered = await _bookService.SearchBooksByCategoryAsync(category);
            if (filtered.Count == 0)
            {
                return NotFound("A megadott kategóriával nem létezik könyv!");
            }
            return Ok(filtered);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookPostDTO book)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdBook = await _bookService.CreateBookAsync(book);
            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookPatchDTO book)
        {
            var updatedBook = await _bookService.UpdateBookAsync(id, book);
            if (updatedBook == null) return NotFound();
            return Ok();
        }

        [HttpPatch("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] BookAvailabilityPatchDTO dto)
        {
            var book = await _bookService.UpdateAvailabilityAsync(dto);

            if (book == null)
            {
                return NotFound("A megadott könyv nem létezik!");
            }

            return Ok();
        }

        [HttpPatch("quality")]
        public async Task<IActionResult> UpdateQuality([FromBody] BookQualityPatchDTO dto)
        {
            var book = await _bookService.UpdateQualityAsync(dto);

            if (book == null)
            {
                return NotFound("A megadott könyv nem létezik!");
            }

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
