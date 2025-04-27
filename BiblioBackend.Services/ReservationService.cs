using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Dtos.Reservation;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiblioBackend.BiblioBackend.Services
{
    public interface IReservationService
    {
        Task<List<ReservationGetDTO>> GetAllReservationsAsync();
        Task<List<ReservationGetDTO>> GetUsersReservationsAsync(string email);
        Task<ReservationDetailDto> GetAllInfoForReservationAsync(int id);
        Task<ReservationGetDTO> CreateReservationAsync(ReservationPostDTO reservation);
        Task<ReservationGetDTO> UpdateReservationAsync(int id, ReservationPatchDTO reservation);
        Task<bool> DeleteReservationAsync(int id);
    }
    public class ReservationService : IReservationService
    {
        private readonly AppDbContext _dbContext;

        public ReservationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<ReservationGetDTO>> GetAllReservationsAsync()
        {
            return await _dbContext.Reservations
                .Select(res => new ReservationGetDTO
                {
                    Id = res.Id,
                    BookId = res.BookId,
                    UserEmail = res.UserEmail,
                    ReservationDate = res.ReservationDate,
                    ExpectedStart = res.ExpectedStart,
                    ExpectedEnd = res.ExpectedEnd
                })
                .ToListAsync();
        }
        public async Task<ReservationDetailDto?> GetAllInfoForReservationAsync(int id)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null) 
            { 
                return null;
            }

            return new ReservationDetailDto
            {
                Id = reservation.Id,
                Book = new BookGetDTO
                {
                    Id = reservation.Book.Id,
                    Title = reservation.Book.Title,
                    AuthorId = reservation.Book.Id,
                    AuthorName = reservation.Book.Author != null ? reservation.Book.Author.Name : null,
                    CategoryId = reservation.Book.Id,
                    CategoryName = reservation.Book.Category != null ? reservation.Book.Category.Name : null,
                    Description = reservation.Book.Description,
                    IsAvailable = reservation.Book.IsAvailable,
                    NumberInLibrary = reservation.Book.NumberInLibrary,
                    BookQuality = reservation.Book.BookQuality
                },
                User = new UserDto
                {
                    Email = reservation.User.Email,
                    FirstName = reservation.User.FirstName,
                    LastName = reservation.User.LastName,
                    Phone = reservation.User.Phone,
                    Address = reservation.User.Address,
                    Privilege = reservation.User.Privilege
                },
                IsAccepted = reservation.IsAccepted,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd
            };
        }
        public async Task<List<ReservationGetDTO>> GetUsersReservationsAsync(string email)
        {
            return await _dbContext.Reservations.Where(res => res.UserEmail == email)
                .Select(res => new ReservationGetDTO
                {
                    Id = res.Id,
                    BookId = res.BookId,
                    UserEmail = res.UserEmail,
                    ReservationDate = res.ReservationDate,
                    ExpectedStart = res.ExpectedStart,
                    ExpectedEnd = res.ExpectedEnd
                })
                .ToListAsync();
        }
        public async Task<ReservationGetDTO> CreateReservationAsync(ReservationPostDTO reservation)
        {
            var newReservation = new Reservation
            {
                BookId = reservation.BookId,
                UserEmail = reservation.UserEmail,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd
            };

            _dbContext.Reservations.Add(newReservation);
            await _dbContext.SaveChangesAsync();

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
        public async Task<ReservationGetDTO> UpdateReservationAsync(int id, ReservationPatchDTO reservation)
        {
            var reservationToUpdate = await _dbContext.Reservations.FindAsync(id);

            if (reservationToUpdate == null)
            {
                return null;
            }

            reservationToUpdate.BookId = reservation.BookId ?? reservationToUpdate.BookId;
            reservationToUpdate.UserEmail = reservation.UserEmail ?? reservationToUpdate.UserEmail;
            reservationToUpdate.IsAccepted = reservation.IsAccepted ?? reservationToUpdate.IsAccepted;
            reservationToUpdate.ReservationDate = reservation.ReservationDate ?? reservationToUpdate.ReservationDate;
            reservationToUpdate.ExpectedStart = reservation.ExpectedStart ?? reservationToUpdate.ExpectedStart;
            reservationToUpdate.ExpectedEnd = reservation.ExpectedEnd ?? reservationToUpdate.ExpectedEnd;

            await _dbContext.SaveChangesAsync();

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
        public async Task<bool> DeleteReservationAsync(int id)
        {
            var toBeDeletedReservation = await _dbContext.Reservations.FindAsync(id);

            if (toBeDeletedReservation == null)
            {
                return false;
            }

            _dbContext.Reservations.Remove(toBeDeletedReservation);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
