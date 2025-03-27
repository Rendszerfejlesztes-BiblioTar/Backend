using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Dtos;
using Microsoft.EntityFrameworkCore;

namespace BiblioBackend.Services
{
    public interface IBookService
    {
        Task<List<BookDto>> GetAllBooksAsync(); // BookDto használata
        Task<Book?> GetBookByIdAsync(int id);
        Task<Book> CreateBookAsync(Book book);
        Task<Book?> UpdateBookAsync(Book book);
        Task<bool> DeleteBookAsync(int id);
        Task<string> GetTest();
        Task<Book?> UpdateAvailabilityAsync(int id, bool available); // Async suffix egységesítve
        Task<Book?> UpdateQualityAsync(int id, BookQuality bookQuality); // Async suffix egységesítve
        Task<List<Book>> SearchBooksByNameAsync(string title);
        Task<List<Book>> SearchBooksByCategoryAsync(string category);
        Task<List<Book>> SearchBooksByAuthorAsync(string author);
    }

    public class BookService : IBookService
    {
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookDto>> GetAllBooksAsync()
        {
            return await _context.Books
                .Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.Name,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category.Name,
                    Description = b.Description,
                    IsAvailable = b.IsAvailable,
                    NumberInLibrary = b.NumberInLibrary,
                    BookQuality = b.BookQuality
                })
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book != null)
            {
                // Ciklus megszakítása
                if (book.Author != null) book.Author.Books = null;
                if (book.Category != null) book.Category.Books = null;
            }

            return book;
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book?> UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books.FindAsync(book.Id);
            if (existingBook == null) return null;

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.NumberInLibrary = book.NumberInLibrary;

            await _context.SaveChangesAsync();
            return existingBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GetTest()
        {
            return "test";
        }

        public async Task<Book?> UpdateAvailabilityAsync(int id, bool isAvailable)
        {
            var bookToUpdate = await _context.Books.FindAsync(id);
            if (bookToUpdate == null) return null;

            bookToUpdate.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();
            return bookToUpdate;
        }

        public async Task<Book?> UpdateQualityAsync(int id, BookQuality bookQuality)
        {
            var bookToUpdate = await _context.Books.FindAsync(id);
            if (bookToUpdate == null) return null;

            bookToUpdate.BookQuality = (int)bookQuality; // Egységes típuskonverzió
            await _context.SaveChangesAsync();
            return bookToUpdate;
        }

        public async Task<List<Book>> SearchBooksByNameAsync(string title)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(title))
                .ToListAsync();

            // Ciklus megszakítása
            foreach (var book in books)
            {
                if (book.Author != null) book.Author.Books = null;
                if (book.Category != null) book.Category.Books = null;
            }

            return books;
        }

        public async Task<List<Book>> SearchBooksByCategoryAsync(string category)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Category.Name.Contains(category))
                .ToListAsync();

            // Ciklus megszakítása
            foreach (var book in books)
            {
                if (book.Author != null) book.Author.Books = null;
                if (book.Category != null) book.Category.Books = null;
            }

            return books;
        }

        public async Task<List<Book>> SearchBooksByAuthorAsync(string author)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Author.Name.Contains(author))
                .ToListAsync();

            // Ciklus megszakítása
            foreach (var book in books)
            {
                if (book.Author != null) book.Author.Books = null;
                if (book.Category != null) book.Category.Books = null;
            }

            return books;
        }
    }
}