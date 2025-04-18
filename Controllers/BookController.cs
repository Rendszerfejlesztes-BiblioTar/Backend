using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// BookController.cs
namespace BiblioBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase //TODO: Fix return types, to only return simple types
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
        private ObjectResult NoPermission => Unauthorized("Nincs jogosultságod ehez!");
        private ObjectResult MissingBook => NotFound("A kért könyv nem létezik!");
        
        // ---

        /// <summary>
        /// Get all books in the database
        /// </summary>
        /// <returns>Every known book</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        /// <summary>
        /// Get a specific book
        /// </summary>
        /// <param name="id">The id of the book</param>
        /// <returns>The requested book</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        /// <summary>
        /// Create a new book
        /// </summary>
        /// <param name="bookValuesDto">The book to create from dto</param>
        /// <returns>The book that was created</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] BookValuesDto bookValuesDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookValuesDto.RequesterEmail);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookValuesDto.RequesterEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            //TODO: maybe dont return the whole book????
            var createdBook = await _bookService.CreateBookAsync(new Book
            {
                Title = bookValuesDto.Title,
                AuthorId = bookValuesDto.AuthorId,
                CategoryId = bookValuesDto.CategoryId,
                Description = bookValuesDto.Description,
                IsAvailable = bookValuesDto.IsAvailable,
                NumberInLibrary = bookValuesDto.NumberInLibrary,
                BookQuality = bookValuesDto.BookQuality
            });
            return CreatedAtAction(nameof(GetBookById), new { id = createdBook.Id }, createdBook);
        }

        /// <summary>
        /// Update a books value
        /// </summary>
        /// <param name="bookSimpleDto">The dto to update from</param>
        /// <returns>The updated book</returns>
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateBook([FromBody] BookSimpleDto bookSimpleDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookSimpleDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookSimpleDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var updatedBook = await _bookService.UpdateBookAsync(new Book
            {
                Id = bookSimpleDto.Id
            });
            
            if (updatedBook == null)
                return MissingBook;
            
            return Ok(updatedBook);
        }

        /// <summary>
        /// Delete a book from the database
        /// </summary>
        /// <param name="bookSimpleDto">The dto to delete from</param>
        /// <returns>True if deleted</returns>
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteBook([FromBody] BookSimpleDto bookSimpleDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookSimpleDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookSimpleDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var result = await _bookService.DeleteBookAsync(bookSimpleDto.Id);
            if (!result) 
                return MissingBook;
            return Ok(result);
        }

        /// <summary>
        /// Set if the book is available for renting
        /// </summary>
        /// <param name="bookAvailablilityDto">The dto from to update the availablity of the book</param>
        /// <returns>The updated book</returns>
        [Authorize]
        [HttpPut("availability")]
        public async Task<IActionResult> UpdateAvailability([FromBody] BookAvailablilityDto bookAvailablilityDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookAvailablilityDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookAvailablilityDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var book = await _bookService.UpdateAvailabilityAsync(bookAvailablilityDto.Id, bookAvailablilityDto.Available);

            if (book == null)
                return MissingBook;

            return Ok(book);
        }

        /// <summary>
        /// Change the quality of a given book
        /// </summary>
        /// <param name="bookQualityDto">The dto from which we update our data</param>
        /// <returns>The updated book</returns>
        [Authorize]
        [HttpPut("quality")]
        public async Task<IActionResult> UpdateQuality([FromBody] BookQualityDto bookQualityDto)
        {
            var isAuthenticated = await UserServiceGeneral.CheckIsUserAuthenticatedAsync(_userService, bookQualityDto.Email);
            if (!isAuthenticated)
                return NotLoggedIn;

            var hasPermssion = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, bookQualityDto.Email, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (hasPermssion)
                return NoPermission;
            
            var book = await _bookService.UpdateQualityAsync(bookQualityDto.Id, bookQualityDto.Quality);

            if (book == null)
                return MissingBook;

            return Ok(book);
        }

        /// <summary>
        /// Search the book by the given title
        /// </summary>
        /// <param name="title">The title to search</param>
        /// <returns>A list of results</returns>
        [HttpGet("search/title")]
        public async Task<IActionResult> SearchBooksByName(string title)
        {
            var filtered = await _bookService.SearchBooksByNameAsync(title);
            if (filtered.Count == 0)
                return MissingBook;
            return Ok(filtered);
        }

        /// <summary>
        /// Search a book by its author
        /// </summary>
        /// <param name="author">The author to search</param>
        /// <returns>The list of results</returns>
        [HttpGet("search/author")]
        public async Task<IActionResult> SearchBooksByAuthor(string author)
        {
            var filtered = await _bookService.SearchBooksByAuthorAsync(author);
            if (filtered.Count == 0)
                return MissingBook;
            return Ok(filtered);
        }

        /// <summary>
        /// Search a book by its category
        /// </summary>
        /// <param name="category">The category to search</param>
        /// <returns>The list of results</returns>
        [HttpGet("search/category")]
        public async Task<IActionResult> SearchBooksByCategory(string category)
        {
            var filtered = await _bookService.SearchBooksByCategoryAsync(category);
            if (filtered.Count == 0)
                return MissingBook;
            return Ok(filtered);
        }
    }
}
