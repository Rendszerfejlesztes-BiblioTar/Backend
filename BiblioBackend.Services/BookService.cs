using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Dtos;
using Microsoft.EntityFrameworkCore;
using BiblioBackend.DataContext.Dtos.Book.Modify;
using BiblioBackend.DataContext.Dtos.Book.Post;
using Microsoft.Extensions.Logging;

namespace BiblioBackend.Services
{
    public interface IBookService
    {
        Task<List<BookGetDto>> GetAllBooksAsync();
        Task<BookGetDto?> GetBookByIdAsync(int id);
        Task<List<BookGetDto>> SearchBooksByNameAsync(string title);
        Task<List<BookGetDto>> SearchBooksByCategoryAsync(string category);
        Task<List<BookGetDto>> SearchBooksByAuthorAsync(string author);
        Task<BookGetDto> CreateBookAsync(BookPostDto book, string requesterEmail);
        Task<BookGetDto?> UpdateBookAsync(int id, BookPatchDto book, string requesterEmail);
        Task<BookGetDto?> UpdateAvailabilityAsync(BookAvailabilityPatchDto dto, string requesterEmail);
        Task<BookGetDto?> UpdateQualityAsync(BookQualityPatchDto dto, string requesterEmail);
        Task<bool> DeleteBookAsync(int id, string requesterEmail);
    }

    public class BookService : IBookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookService> _logger;

        public BookService(AppDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BookGetDto>> GetAllBooksAsync()
        {
            _logger.LogInformation("Retrieving all books");
            return await _context.Books
                .Select(b => new BookGetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .ToListAsync();
        }

        public async Task<BookGetDto?> GetBookByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving book with ID {Id}", id);
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Select(b => new BookGetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .FirstOrDefaultAsync(b => b.Id == id);

            return book;
        }

        public async Task<List<BookGetDto>> SearchBooksByNameAsync(string title)
        {
            _logger.LogInformation("Searching books by title: {Title}", title);
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(title))
                .Select(b => new BookGetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .ToListAsync();
        }

        public async Task<List<BookGetDto>> SearchBooksByCategoryAsync(string category)
        {
            _logger.LogInformation("Searching books by category: {Category}", category);
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Category != null && b.Category.Name != null && b.Category.Name.Contains(category))
                .Select(b => new BookGetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .ToListAsync();
        }

        public async Task<List<BookGetDto>> SearchBooksByAuthorAsync(string author)
        {
            _logger.LogInformation("Searching books by author: {Author}", author);
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Author != null && b.Author.Name != null && b.Author.Name.Contains(author))
                .Select(b => new BookGetDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author != null ? b.Author.Name : null,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category != null ? b.Category.Name : null,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .ToListAsync();
        }

        public async Task<BookGetDto> CreateBookAsync(BookPostDto book, string requesterEmail)
        {
            _logger.LogInformation("Creating book by {Email}", requesterEmail);
            var newBook = new Book
            {
                Title = book.Title,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                Description = book.Description,
                IsAvailable = book.IsAvailable,
                NumberInLibrary = book.NumberInLibrary,
                BookQuality = book.BookQuality
            };

            _context.Books.Add(newBook);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book {Title} created by {Email}", book.Title, requesterEmail);

            return new BookGetDto
            {
                Id = newBook.Id,
                Title = newBook.Title,
                AuthorId = newBook.AuthorId,
                AuthorName = newBook.Author?.Name,
                CategoryId = newBook.CategoryId,
                CategoryName = newBook.Category?.Name,
                Description = newBook.Description,
                IsAvailable = newBook.IsAvailable,
                NumberInLibrary = newBook.NumberInLibrary,
                BookQuality = newBook.BookQuality
            };
        }

        public async Task<BookGetDto?> UpdateBookAsync(int id, BookPatchDto book, string requesterEmail)
        {
            _logger.LogInformation("Updating book {Id} by {Email}", id, requesterEmail);
            var existingBook = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (existingBook == null)
            {
                _logger.LogWarning("Book {Id} not found", id);
                return null;
            }

            existingBook.Title = book.Title ?? existingBook.Title;
            existingBook.AuthorId = book.AuthorId ?? existingBook.AuthorId;
            existingBook.CategoryId = book.CategoryId ?? existingBook.CategoryId;
            existingBook.Description = book.Description ?? existingBook.Description;
            existingBook.IsAvailable = book.IsAvailable ?? existingBook.IsAvailable;
            existingBook.NumberInLibrary = book.NumberInLibrary ?? existingBook.NumberInLibrary;
            existingBook.BookQuality = book.BookQuality != null ? (BookQuality)book.BookQuality : existingBook.BookQuality;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Book {Id} updated by {Email}", id, requesterEmail);

            return new BookGetDto
            {
                Id = existingBook.Id,
                Title = existingBook.Title,
                AuthorId = existingBook.AuthorId,
                AuthorName = existingBook.Author?.Name,
                CategoryId = existingBook.CategoryId,
                CategoryName = existingBook.Category?.Name,
                Description = existingBook.Description,
                IsAvailable = existingBook.IsAvailable,
                NumberInLibrary = existingBook.NumberInLibrary,
                BookQuality = existingBook.BookQuality
            };
        }

        public async Task<BookGetDto?> UpdateAvailabilityAsync(BookAvailabilityPatchDto dto, string requesterEmail)
        {
            _logger.LogInformation("Updating availability for book {Id} by {Email}", dto.Id, requesterEmail);
            var bookToUpdate = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == dto.Id);

            if (bookToUpdate == null)
            {
                _logger.LogWarning("Book {Id} not found", dto.Id);
                return null;
            }

            bookToUpdate.IsAvailable = dto.IsAvailable;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Availability updated for book {Id} by {Email}", dto.Id, requesterEmail);

            return new BookGetDto
            {
                Id = bookToUpdate.Id,
                Title = bookToUpdate.Title,
                AuthorId = bookToUpdate.AuthorId,
                AuthorName = bookToUpdate.Author?.Name,
                CategoryId = bookToUpdate.CategoryId,
                CategoryName = bookToUpdate.Category?.Name,
                Description = bookToUpdate.Description,
                IsAvailable = bookToUpdate.IsAvailable,
                NumberInLibrary = bookToUpdate.NumberInLibrary,
                BookQuality = bookToUpdate.BookQuality
            };
        }

        public async Task<BookGetDto?> UpdateQualityAsync(BookQualityPatchDto dto, string requesterEmail)
        {
            _logger.LogInformation("Updating quality for book {Id} by {Email}", dto.Id, requesterEmail);
            var bookToUpdate = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == dto.Id);

            if (bookToUpdate == null)
            {
                _logger.LogWarning("Book {Id} not found", dto.Id);
                return null;
            }

            bookToUpdate.BookQuality = dto.BookQuality;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Quality updated for book {Id} by {Email}", dto.Id, requesterEmail);

            return new BookGetDto
            {
                Id = bookToUpdate.Id,
                Title = bookToUpdate.Title,
                AuthorId = bookToUpdate.AuthorId,
                AuthorName = bookToUpdate.Author?.Name,
                CategoryId = bookToUpdate.CategoryId,
                CategoryName = bookToUpdate.Category?.Name,
                Description = bookToUpdate.Description,
                IsAvailable = bookToUpdate.IsAvailable,
                NumberInLibrary = bookToUpdate.NumberInLibrary,
                BookQuality = bookToUpdate.BookQuality
            };
        }

        public async Task<bool> DeleteBookAsync(int id, string requesterEmail)
        {
            _logger.LogInformation("Deleting book {Id} by {Email}", id, requesterEmail);
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Book {Id} not found", id);
                return false;
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book {Id} deleted by {Email}", id, requesterEmail);
            return true;
        }
    }
}