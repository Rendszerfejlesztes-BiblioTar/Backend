using BiblioBackend.Services;
using Microsoft.AspNetCore.Mvc;
namespace BiblioBackend.Controllers;

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
    public async Task<IActionResult> GetTest()
    {
        var test = await _bookService.GetTest();
        return Ok(test);
    }
}