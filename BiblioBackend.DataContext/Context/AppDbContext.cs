using System.Runtime.InteropServices;
using BiblioBackend.BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
namespace BiblioBackend.DataContext.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Loan> Loans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connectionString;
    
        connectionString = "Server=localhost;Database=Biblio;Trusted_Connection=True;TrustServerCertificate=True;";

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

        modelBuilder.Entity<Loan>()
            .HasOne(l => l.User)
            .WithMany(u => u.Loans)
            .HasForeignKey(l => l.UserEmail)
            .HasPrincipalKey(u => u.Email);

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reservations)
            .HasForeignKey(r => r.UserEmail)
            .HasPrincipalKey(u => u.Email);

        // Book relationships
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId);

        modelBuilder.Entity<Book>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId);
    }
}