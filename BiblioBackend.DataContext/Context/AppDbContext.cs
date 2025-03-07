using System.Runtime.InteropServices;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
namespace BiblioBackend.DataContext.Context;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<LoanHistory> LoanHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Muszály egy külön db connection string, nincs windows auth linuxon
            connectionString = "Server=localhost;Initial Catalog=Biblio;User Id=SA;Password=Database1234;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;";
        }
        else
        {
            connectionString = "Server=localhost;Database=Biblio;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.Email);

        modelBuilder.Entity<User>()
            .Property(u => u.Privilege)
            .HasConversion<string>();

        // Book relationships
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId);

        // Reservation relationships
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Book)
            .WithMany()
            .HasForeignKey(r => r.BookId);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserEmail);

        // Loan history relationships
        modelBuilder.Entity<LoanHistory>()
            .HasOne(l => l.Book)
            .WithMany()
            .HasForeignKey(l => l.BookId);

        modelBuilder.Entity<LoanHistory>()
            .HasOne(l => l.User)
            .WithMany(u => u.LoanHistories)
            .HasForeignKey(l => l.UserEmail);
    }
}