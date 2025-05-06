using BiblioBackend.BiblioBackend.Services;
using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BiblioBackend.Services
{
    public interface IReservationService
    {
        Task<List<ReservationGetDTO>> GetAllReservationsAsync();
        Task<List<ReservationGetDTO>> GetUsersReservationsAsync(string email);
        Task<ReservationDetailDto?> GetAllInfoForReservationAsync(int id);
        Task<ReservationGetDTO> CreateReservationAsync(ReservationPostDTO reservation, string userEmail);
        Task<ReservationGetDTO> UpdateReservationAsync(int id, ReservationPatchDTO reservation, string userEmail);
        Task<bool> DeleteReservationAsync(int id, string userEmail);
    }

    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<ReservationService> _logger;
        private readonly IUserService _userService; // Added for permission checks

        public ReservationService(AppDbContext dbContext, ILogger<ReservationService> logger, IUserService userService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
        }

        public async Task<List<ReservationGetDTO>> GetAllReservationsAsync()
        {
            _logger.LogInformation("Retrieving all reservations");
            return await _dbContext.Reservations
                .Select(res => new ReservationGetDTO
                {
                    Id = res.Id,
                    BookId = res.BookId,
                    UserEmail = res.UserEmail,
                    IsAccepted = res.IsAccepted,
                    ReservationDate = res.ReservationDate,
                    ExpectedStart = res.ExpectedStart,
                    ExpectedEnd = res.ExpectedEnd
                })
                .ToListAsync();
        }

        public async Task<List<ReservationGetDTO>> GetUsersReservationsAsync(string email)
        {
            _logger.LogInformation("Retrieving reservations for user {Email}", email);
            return await _dbContext.Reservations
                .Where(res => res.UserEmail == email)
                .Select(res => new ReservationGetDTO
                {
                    Id = res.Id,
                    BookId = res.BookId,
                    UserEmail = res.UserEmail,
                    IsAccepted = res.IsAccepted,
                    ReservationDate = res.ReservationDate,
                    ExpectedStart = res.ExpectedStart,
                    ExpectedEnd = res.ExpectedEnd
                })
                .ToListAsync();
        }

        public async Task<ReservationDetailDto?> GetAllInfoForReservationAsync(int id)
        {
            _logger.LogInformation("Retrieving details for reservation {Id}", id);
            var reservation = await _dbContext.Reservations
                .Include(r => r.Book).ThenInclude(book => book.Author)
                .Include(r => r.User).Include(reservation => reservation.Book).ThenInclude(book => book.Category)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
            {
                _logger.LogWarning("Reservation {Id} not found", id);
                return null;
            }

            return new ReservationDetailDto
            {
                Id = reservation.Id,
                Book = new BookGetDTO
                {
                    Id = reservation.Book?.Id ?? 0,
                    Title = reservation.Book?.Title ?? "",
                    AuthorId = reservation.Book?.AuthorId ?? 0,
                    AuthorName = reservation.Book?.Author?.Name,
                    CategoryId = reservation.Book?.CategoryId ?? 0,
                    CategoryName = reservation.Book?.Category?.Name,
                    Description = reservation.Book?.Description,
                    IsAvailable = reservation.Book?.IsAvailable ?? false,
                    NumberInLibrary = reservation.Book?.NumberInLibrary,
                    BookQuality = reservation.Book?.BookQuality ?? BookQuality.Poor
                },
                User = new UserDto
                {
                    Email = reservation.User?.Email ?? "",
                    FirstName = reservation.User?.FirstName,
                    LastName = reservation.User?.LastName,
                    Phone = reservation.User?.Phone,
                    Address = reservation.User?.Address,
                    Privilege = reservation.User?.Privilege ?? PrivilegeLevel.UnRegistered
                },
                IsAccepted = reservation.IsAccepted,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd
            };
        }

        public async Task<ReservationGetDTO> CreateReservationAsync(ReservationPostDTO reservation, string userEmail)
        {
            _logger.LogInformation("Creating reservation for user {Email}", userEmail);

            // Validate book existence
            var book = await _dbContext.Books.FindAsync(reservation.BookId);
            if (book == null)
            {
                _logger.LogWarning("Cannot create reservation: Book {BookId} not found", reservation.BookId);
                throw new InvalidOperationException("Book not found.");
            }

            var newReservation = new Reservation
            {
                BookId = reservation.BookId,
                UserEmail = userEmail,
                IsAccepted = reservation.IsAccepted,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd
            };

            _dbContext.Reservations.Add(newReservation);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Reservation created for user {Email}, book {BookId}", userEmail, reservation.BookId);

            return new ReservationGetDTO
            {
                Id = newReservation.Id,
                UserEmail = newReservation.UserEmail,
                BookId = newReservation.BookId,
                IsAccepted = newReservation.IsAccepted,
                ReservationDate = newReservation.ReservationDate,
                ExpectedStart = newReservation.ExpectedStart,
                ExpectedEnd = newReservation.ExpectedEnd
            };
        }

        public async Task<ReservationGetDTO> UpdateReservationAsync(int id, ReservationPatchDTO reservation, string userEmail)
        {
            _logger.LogInformation("Updating reservation {Id} by {Email}", id, userEmail);
            var reservationToUpdate = await _dbContext.Reservations.FindAsync(id);

            if (reservationToUpdate == null)
            {
                _logger.LogWarning("Reservation {Id} not found", id);
                throw new KeyNotFoundException($"Reservation with ID {id} not found.");
            }

            // Validate user permission
            var isAdminOrLibrarian = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!isAdminOrLibrarian && reservationToUpdate.UserEmail != userEmail)
            {
                _logger.LogWarning("User {Email} cannot update reservation {Id} belonging to another user", userEmail, id);
                throw new UnauthorizedAccessException("Cannot update another user's reservation.");
            }

            reservationToUpdate.BookId = reservation.BookId ?? reservationToUpdate.BookId;
            reservationToUpdate.IsAccepted = reservation.IsAccepted ?? reservationToUpdate.IsAccepted;
            reservationToUpdate.ReservationDate = reservation.ReservationDate ?? reservationToUpdate.ReservationDate;
            reservationToUpdate.ExpectedStart = reservation.ExpectedStart ?? reservationToUpdate.ExpectedStart;
            reservationToUpdate.ExpectedEnd = reservation.ExpectedEnd ?? reservationToUpdate.ExpectedEnd;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Reservation {Id} updated by {Email}", id, userEmail);

            return new ReservationGetDTO
            {
                Id = reservationToUpdate.Id,
                UserEmail = reservationToUpdate.UserEmail,
                BookId = reservationToUpdate.BookId,
                IsAccepted = reservationToUpdate.IsAccepted,
                ReservationDate = reservationToUpdate.ReservationDate,
                ExpectedStart = reservationToUpdate.ExpectedStart,
                ExpectedEnd = reservationToUpdate.ExpectedEnd
            };
        }

        public async Task<bool> DeleteReservationAsync(int id, string userEmail)
        {
            _logger.LogInformation("Deleting reservation {Id} by {Email}", id, userEmail);
            var toBeDeletedReservation = await _dbContext.Reservations.FindAsync(id);

            if (toBeDeletedReservation == null)
            {
                _logger.LogWarning("Reservation {Id} not found", id);
                return false;
            }

            // Validate user permission
            var isAdminOrLibrarian = await UserServiceGeneral.CheckIsUserPermittedAsync(_userService, userEmail, PrivilegeLevel.Admin, PrivilegeLevel.Librarian);
            if (!isAdminOrLibrarian && toBeDeletedReservation.UserEmail != userEmail)
            {
                _logger.LogWarning("User {Email} cannot delete reservation {Id} belonging to another user", userEmail, id);
                throw new UnauthorizedAccessException("Cannot delete another user's reservation.");
            }

            _dbContext.Reservations.Remove(toBeDeletedReservation);
            await _dbContext.SaveChangesAsync(); // Fixed from _context

            _logger.LogInformation("Reservation {Id} deleted by {Email}", id, userEmail);
            return true;
        }
    }
}