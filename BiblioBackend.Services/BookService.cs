using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Dtos;
using Microsoft.EntityFrameworkCore;
using BiblioBackend.DataContext.Dtos.Book.Modify;
using BiblioBackend.DataContext.Dtos.Book.Post;

namespace BiblioBackend.Services
{
    public interface IBookService
    {
        Task<List<BookGetDTO>> GetAllBooksAsync();
        Task<BookGetDTO?> GetBookByIdAsync(int id);
        Task<List<BookGetDTO>> SearchBooksByNameAsync(string title);
        Task<List<BookGetDTO>> SearchBooksByCategoryAsync(string category);
        Task<List<BookGetDTO>> SearchBooksByAuthorAsync(string author);
        Task<BookPostDTO> CreateBookAsync(BookPostDTO book);
        Task<BookGetDTO?> UpdateBookAsync(int id, BookPatchDTO book);
        Task<BookGetDTO?> UpdateAvailabilityAsync(BookAvailabilityPatchDTO dto);
        Task<BookGetDTO?> UpdateQualityAsync(BookQualityPatchDTO dto);
        Task<bool> DeleteBookAsync(int id);
    }

    public class BookService : IBookService
    {
        private readonly AppDbContext _context;

        public BookService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookGetDTO>> GetAllBooksAsync()
        {
            return await _context.Books
                .Select(b => new BookGetDTO
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

        public async Task<BookGetDTO?> GetBookByIdAsync(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Select(b => new BookGetDTO
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

        public async Task<List<BookGetDTO>> SearchBooksByNameAsync(string title)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Title.Contains(title))
                .Select(b => new BookGetDTO
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


            return books;
        }

        public async Task<List<BookGetDTO>> SearchBooksByCategoryAsync(string category)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Category.Name.Contains(category))
                .Select(b => new BookGetDTO
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

            return books;
        }

        public async Task<List<BookGetDTO>> SearchBooksByAuthorAsync(string author)
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .Where(b => b.Author.Name.Contains(author))
                .Select(b => new BookGetDTO
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

            return books;
        }

        public async Task<BookPostDTO> CreateBookAsync(BookPostDTO book)
        {
            _context.Books.Add(new Book {
                Title = book.Title,
                AuthorId = book.AuthorId,
                CategoryId = book.CategoryId,
                Description = book.Description,
                IsAvailable = book.IsAvailable,
                NumberInLibrary = book.NumberInLibrary,
                BookQuality = book.BookQuality
            });
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<BookGetDTO?> UpdateBookAsync(int id, BookPatchDTO book)
        {
            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null) return null;

            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.NumberInLibrary = book.NumberInLibrary;

            await _context.SaveChangesAsync();
            return new BookGetDTO
            {
                Id = existingBook.Id,
                Title = existingBook.Title,
                AuthorId = existingBook.AuthorId,
                AuthorName = existingBook.Author != null ? existingBook.Author.Name : null,
                CategoryId = existingBook.CategoryId,
                CategoryName = existingBook.Category != null ? existingBook.Category.Name : null,
                Description = existingBook.Description,
                IsAvailable = existingBook.IsAvailable,
                NumberInLibrary = existingBook.NumberInLibrary,
                BookQuality = existingBook.BookQuality
            }; ;
        }

        public async Task<BookGetDTO?> UpdateAvailabilityAsync(BookAvailabilityPatchDTO dto)
        {
            var bookToUpdate = await _context.Books.FindAsync(dto.Id);
            if (bookToUpdate == null) return null;

            bookToUpdate.IsAvailable = dto.IsAvailable;
            await _context.SaveChangesAsync();
            return new BookGetDTO
            {
                Id = bookToUpdate.Id,
                Title = bookToUpdate.Title,
                AuthorId = bookToUpdate.AuthorId,
                AuthorName = bookToUpdate.Author != null ? bookToUpdate.Author.Name : null,
                CategoryId = bookToUpdate.CategoryId,
                CategoryName = bookToUpdate.Category != null ? bookToUpdate.Category.Name : null,
                Description = bookToUpdate.Description,
                IsAvailable = bookToUpdate.IsAvailable,
                NumberInLibrary = bookToUpdate.NumberInLibrary,
                BookQuality = bookToUpdate.BookQuality
            };
        }

        public async Task<BookGetDTO?> UpdateQualityAsync(BookQualityPatchDTO dto)
        {
            var bookToUpdate = await _context.Books.FindAsync(dto.Id);
            if (bookToUpdate == null) return null;

            bookToUpdate.BookQuality = dto.BookQuality;
            await _context.SaveChangesAsync();
            return new BookGetDTO
            {
                Id = bookToUpdate.Id,
                Title = bookToUpdate.Title,
                AuthorId = bookToUpdate.AuthorId,
                AuthorName = bookToUpdate.Author != null ? bookToUpdate.Author.Name : null,
                CategoryId = bookToUpdate.CategoryId,
                CategoryName = bookToUpdate.Category != null ? bookToUpdate.Category.Name : null,
                Description = bookToUpdate.Description,
                IsAvailable = bookToUpdate.IsAvailable,
                NumberInLibrary = bookToUpdate.NumberInLibrary,
                BookQuality = bookToUpdate.BookQuality
            };
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}