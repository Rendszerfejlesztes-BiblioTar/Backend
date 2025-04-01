using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Adatbázis kontextus regisztrálása
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Szolgáltatások regisztrálása
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IUserService, UserService>();


// Swagger konfiguráció
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bibliotár API", Version = "v1" });
});

var app = builder.Build();

// Adatbázis inicializálása és példa adatok feltöltése
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Biztosítjuk, hogy az adatbázis létezzen (migrációk alapján)
    dbContext.Database.Migrate();

    // Példa adatok feltöltése
    await SeedDatabaseAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bibliotár API v1"));
}

app.UseCors("AllowLocalhost");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Segédfüggvény az adatbázis feltöltéséhez
async Task SeedDatabaseAsync(AppDbContext dbContext)
{
    // Változók deklarálása a metódus elején
    List<Author> authors = dbContext.Authors.ToList();
    List<Category> categories = dbContext.Categories.ToList();

    // Szerzők feltöltése, ha még nincsenek
    if (!authors.Any())
    {
        authors = new List<Author>
        {
            new Author { Name = "J.K. Rowling" },
            new Author { Name = "George R.R. Martin" },
            new Author { Name = "J.R.R. Tolkien" }
        };
        dbContext.Authors.AddRange(authors);
        await dbContext.SaveChangesAsync();
    }

    // Kategóriák feltöltése, ha még nincsenek
    if (!categories.Any())
    {
        categories = new List<Category>
        {
            new Category { Name = "Fantasy" },
            new Category { Name = "Adventure" },
            new Category { Name = "Mystery" }
        };
        dbContext.Categories.AddRange(categories);
        await dbContext.SaveChangesAsync();
    }

    // Könyvek feltöltése, csak ha új könyv (cím alapján ellenőrizve)
    var existingBookTitles = dbContext.Books.Select(b => b.Title).ToHashSet();
    var booksToAdd = new List<Book>
    {
        new Book
        {
            Title = "Harry Potter and the Philosopher's Stone",
            AuthorId = authors.First(a => a.Name == "J.K. Rowling").Id,
            CategoryId = categories.First(c => c.Name == "Fantasy").Id,
            Description = "A young wizard's journey begins.",
            IsAvailable = true,
            NumberInLibrary = "B001"
        },
        new Book
        {
            Title = "A Game of Thrones",
            AuthorId = authors.First(a => a.Name == "George R.R. Martin").Id,
            CategoryId = categories.First(c => c.Name == "Adventure").Id,
            Description = "Epic tale of kings and dragons.",
            IsAvailable = false,
            NumberInLibrary = "B002"
        },
        new Book
        {
            Title = "The Hobbit",
            AuthorId = authors.First(a => a.Name == "J.R.R. Tolkien").Id,
            CategoryId = categories.First(c => c.Name == "Fantasy").Id,
            Description = "A hobbit's adventure.",
            IsAvailable = true,
            NumberInLibrary = "B003"
        }
    };

    var newBooks = booksToAdd.Where(b => !existingBookTitles.Contains(b.Title)).ToList();
    if (newBooks.Any())
    {
        dbContext.Books.AddRange(newBooks);
        await dbContext.SaveChangesAsync();
    }

    // Felhasználók, foglalások és kölcsönzési előzmények nem kerülnek itt feltöltésre,
    // hogy a regisztrált felhasználók adatai megmaradjanak
}