using BiblioBackend.DataContext.Context;

namespace BiblioBackend.Services;

public interface IBookService
{
    Task<string> GetTest();
}

public class BookService : IBookService
{
    private readonly AppDbContext _context;

    public BookService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<string> GetTest()
    {
        return "test";
    }
}