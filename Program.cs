using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using AspNetCoreRateLimit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiblioBackend.BiblioBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve PascalCase for frontend compatibility
    });

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Add HttpContextAccessor for accessing HTTP context in services
builder.Services.AddHttpContextAccessor();

// Add Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Add Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserAccess", policy =>
        policy.RequireRole(
            PrivilegeLevel.Registered.ToString(),
            PrivilegeLevel.Librarian.ToString(),
            PrivilegeLevel.Admin.ToString()));
    options.AddPolicy("AdminAccess", policy =>
        policy.RequireRole(PrivilegeLevel.Admin.ToString()));
});

// Add CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", builder =>
    {
        builder.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Add Rate Limiting with AspNetCoreRateLimit
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Add Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bibliotár API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); // Apply migrations
    await SeedDatabaseAsync(dbContext); // Seed Authors, Categories, Books
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bibliotár API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.UseIpRateLimiting(); // Use AspNetCoreRateLimit middleware
app.MapControllers();

// Global Exception Handling to prevent HTTP 500 errors
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred." });
    });
});

app.Run();

// Seed database with example data
async Task SeedDatabaseAsync(AppDbContext dbContext)
{
    List<Author> authors = dbContext.Authors.ToList();
    List<Category> categories = dbContext.Categories.ToList();

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
    
    var existingUsers = dbContext.Users.Select(b => b.Email).ToHashSet();
    var usersToAdd = new List<User>
    {
        new User
        {
            Email = "admin@admin.com",
            Address = "Admin",
            FirstName = "Kovács",
            LastName = "István",
            PasswordHash = "Admin123",
            Phone = "062012345678",
            Privilege = PrivilegeLevel.Admin
        }
    };
    var defaultAdmin = usersToAdd.Where(b => !existingUsers.Contains(b.Email)).ToList();
    if (defaultAdmin.Any())
    {
        dbContext.Users.AddRange(defaultAdmin);
        await dbContext.SaveChangesAsync();
    }
    // Default admin user létrehozása
}