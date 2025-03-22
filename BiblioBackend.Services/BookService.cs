using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiblioBackend.Services
{
    namespace BiblioBackend.Services
    {
        public interface IBookService
        {
            Task<List<Book>> GetAllBooksAsync();
            Task<Book?> GetBookByIdAsync(int id); // Nullable Book
            Task<Book> CreateBookAsync(Book book);
            Task<Book?> UpdateBookAsync(Book book); // Nullable Book
            Task<bool> DeleteBookAsync(int id);
            Task<string> GetTest();
        }

        public class BookService : IBookService
        {
            private readonly AppDbContext _context;

            public BookService(AppDbContext context)
            {
                _context = context;
            }

            public async Task<List<Book>> GetAllBooksAsync()
            {
                return await _context.Books.ToListAsync();
            }

            public async Task<Book?> GetBookByIdAsync(int id)
            {
                return await _context.Books.FindAsync(id);
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
                existingBook.IsAvailable = book.IsAvailable;
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
        }
    }
}