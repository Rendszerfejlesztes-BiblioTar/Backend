using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;

// BookController.cs
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

        [HttpGet("test")]
        public async Task<IActionResult> GetTest()
        {
            var test = await _bookService.GetTest();
            return Ok(test);
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

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] Book book)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var createdBook = await _bookService.CreateBookAsync(book);
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] Book book)
        {
            if (id != book.Id) return BadRequest("Az ID nem egyezik a k�r�ssel.");
            var updatedBook = await _bookService.UpdateBookAsync(book);
            if (updatedBook == null) return NotFound();
            return Ok(updatedBook);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPut("availability/{id}")]
        public async Task<IActionResult> UpdateAvailability(int id, bool isAvailable)
        {
            var book = await _bookService.UpdateAvailability(id, isAvailable);

            if (book == null)
            {
                return NotFound("A megadott k�nyv nem l�tezik!");
            }

            return Ok(book);
        }

        [HttpPut("quality/{id}")]
        public async Task<IActionResult> UpdateQuality(int id, BookQuality bookQuality)
        {
            var book = await _bookService.UpdateQuality(id, bookQuality);

            if (book == null)
            {
                return NotFound("A megadott k�nyv nem l�tezik!");
            }

            return Ok(book);
        }
    }
}
