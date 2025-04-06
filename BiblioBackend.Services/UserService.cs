using BiblioBackend.BiblioBackend.DataContext.Entities;
using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.DataContext.Dtos.User.Post;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;

namespace BiblioBackend.Services;

public interface IUserService
{
    // Get      ------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Check if the given email is tied to an existing user
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>The result of the requested action (true for exists)</returns>
    Task<bool> GetUserIsExistsAsync(string email);

    /// <summary>
    /// Check if the given email is authenticated or not
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>The result of the requested action (true for authenticated)</returns>
    Task<bool> GetUserAuthenticatedAsync(string email);
    
    /// <summary>
    /// Get the users contact information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<UserGetContactDTO> GetUserContactInformationByEmailAsync(string email);
    
    /// <summary>
    /// Get the users privilege information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<UserGetPrivilegeLevelDTO> GetUserPrivilegeLevelByEmailAsync(string email);
    
    /// <summary>
    /// Get the users reservation information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserReservationDTO>> GetUserReservationsByEmailAsync(string email);
    
    /// <summary>
    /// Get the users reservations by the given IDs
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <param name="userReservationDto">The dto containing the information about the reservations to get</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserReservationDTO>> GetUserSelectedReservationsByEmailAsync(string email, List<UserReservationDTO> userReservationDto);
    
    /// <summary>
    /// Get the users loan information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserLoanDTO>> GetUserLoansByEmailAsync(string email);
    
    /// <summary>
    /// Get the users loans by the given IDs
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <param name="userLoanDto">The dto containing the information about the loans to get</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserLoanDTO>> GetUserSelectedLoansByEmailAsync(string email, List<UserLoanDTO> userLoanDto);
    
    // Put     ------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Modify the users contact information
    /// </summary>
    /// <param name="userModifyContactDto">The DTO from which the user contacts will be changed to</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserContactInformationAsync(UserModifyContactDTO userModifyContactDto);
    
    /// <summary>
    /// Modify the users email information, changing its primary id!!!
    /// Proper checks and precautions must be applied!
    /// </summary>
    /// <param name="userModifyLoginDto">The DTO from which the user email will be changed to</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserLoginAsync(UserModifyLoginDTO userModifyLoginDto);
    
    /// <summary>
    /// Add the reservations from the given DTO to the user
    /// </summary>
    /// <param name="email">The email of the target user</param>
    /// <param name="userModifyReservationDto">The DTO from which the new reservations are from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserReservationsAsync(string email, List<UserReservationDTO> userModifyReservationDto);
    
    /// <summary>
    /// Add the loans from the given DTO to the user
    /// </summary>
    /// <param name="email">The email of the target user</param>
    /// <param name="userModifyLoanDto">The DTO from which the new loans are from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserLoansAsync(string email, List<UserLoanDTO> userModifyLoanDto);
    
    /// <summary>
    /// Changes the users privilege level
    /// This should only be allowed by an administrator!!!
    /// </summary>
    /// <param name="userModifyPrivilegeDto">The DTO from which the new privilege is from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserPrivilegeAsync(UserModifyPrivilegeDTO userModifyPrivilegeDto);
    
    // Delete   ------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Delete the user from the database, can not be undone!!!
    /// </summary>
    /// <param name="email">The email of the user we want to delete</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> RemoveUserAsync(string email);
    
    /// <summary>
    /// Remove the reservations from the given DTO from the user
    /// </summary>
    /// <param name="email">The email of the target user</param>
    /// <param name="userModifyReservationDto">The DTO from which we are removing from the user</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> RemoveUserReservationAsync(string email, List<UserReservationDTO> userModifyReservationDto);
    
    /// <summary>
    /// Remove the loans from the given DTO from the user
    /// </summary>
    /// <param name="email">The email of the target user</param>
    /// <param name="userModifyLoanDto">The DTO from which we are removing from the user</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> RemoveUserLoansAsync(string email, List<UserLoanDTO> userModifyLoanDto);
    
    // Post     ------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Authenticate the given user from the DTO
    /// </summary>
    /// <param name="userLoginValuesDto">The DTO contains the users data, from the frontend</param>
    /// <returns>A DTO with their auth token</returns>
    Task<UserLoginTokenDTO> PostAutenticationAsync(UserLoginValuesDTO userLoginValuesDto);
    
    /// <summary>
    /// Create a user from the DTO
    /// </summary>
    /// <param name="userLoginValuesDto">The DTO contains the users data, from the frontend</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> PostUserCreateAsync(UserLoginValuesDTO userLoginValuesDto);
}

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // ---

    public async Task<bool> GetUserIsExistsAsync(string email)
    {
        if (email.Trim() == string.Empty)
        {
            return false;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        return user != null;
    }

    public async Task<bool> GetUserAuthenticatedAsync(string email)
    {
        //TODO: create authentication token verification
        
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // Unregistered user
            Console.WriteLine($"[UserService::GetUserAuthenticatedAsync] User not found! Email: {email}");
            return false;
        }
        
        // User existing in db
        return true;
    }

    public async Task<UserGetContactDTO> GetUserContactInformationByEmailAsync(string email)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return null;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid or not registered
            Console.WriteLine($"[UserService::GetUserContactInformationByEmailAsync] User not found! Email: {email}");
            return new UserGetContactDTO()
            {
                FirstName = "",
                LastName = "",
                Phone = "",
                Address = "",
            };
        }
        // Otherwise, give back real user data
        return new UserGetContactDTO()
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone,
            Address = user.Address,
        };
    }

    public async Task<UserGetPrivilegeLevelDTO> GetUserPrivilegeLevelByEmailAsync(string email)
    {
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid, or not registered
            Console.WriteLine($"[UserService::GetUserPrivilegeLevelByEmailAsync] User not found! Email: {email}");
            return new UserGetPrivilegeLevelDTO()
            {
                Privilege = PrivilegeLevel.UnRegistered,
                PrivilegeString = PrivilegeLevelStringify.PrivilegeLevelStrings[PrivilegeLevel.UnRegistered.ToString()]
            };
        }
        // Otherwise, give back real user data
        return new UserGetPrivilegeLevelDTO()
        {
            Privilege = user.Privilege,
            PrivilegeString = PrivilegeLevelStringify.PrivilegeLevelStrings[user.Privilege.ToString()]
        };
    }

    public async Task<List<UserReservationDTO>> GetUserReservationsByEmailAsync(string email)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return null;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Reservations).FirstOrDefaultAsync();
        
        var result = new List<UserReservationDTO>();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::GetUserReservationsByEmailAsync] User not found! Email: {email}");
            return result;
        }

        var reservations = user.Reservations;
        if (reservations == null)
        {
            // No reservations
            Console.WriteLine($"[UserService::GetUserReservationsByEmailAsync] Reservations are empty! Email: {email}");
            return result;
        }

        Parallel.ForEach(reservations, reservation =>
        {
            result.Add(new UserReservationDTO()
            { 
                ReservationId = reservation.Id,
                BookId = reservation.BookId,
                IsAccepted = reservation.IsAccepted,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd,
            });
        });

        return result;
    }

    public async Task<List<UserReservationDTO>> GetUserSelectedReservationsByEmailAsync(string email, List<UserReservationDTO> userReservationDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return null;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Reservations).FirstOrDefaultAsync();
        
        var result = new List<UserReservationDTO>();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::GetUserSelectedReservationsAsync] User not found! Email: {email}");
            return result;
        }

        var reservations = user.Reservations;
        if (reservations == null)
        {
            // No reservations
            Console.WriteLine($"[UserService::GetUserSelectedReservationsAsync] Reservations are empty! Email: {email}");
            return result;
        }
        
        var convertId = userReservationDto.ConvertAll(o => o.ReservationId);

        var lookupReservations = reservations.Where(r => convertId.Contains(r.Id)).ToList();

        Parallel.ForEach(lookupReservations, reservation =>
        {
            result.Add(new UserReservationDTO()
            { 
                ReservationId = reservation.Id,
                BookId = reservation.BookId,
                IsAccepted = reservation.IsAccepted,
                ReservationDate = reservation.ReservationDate,
                ExpectedStart = reservation.ExpectedStart,
                ExpectedEnd = reservation.ExpectedEnd,
            });
        });

        return result;
    }

    public async Task<List<UserLoanDTO>> GetUserLoansByEmailAsync(string email)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return null;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Loans).FirstOrDefaultAsync();
        
        var result = new List<UserLoanDTO>();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::GetUserLoansByEmailAsync] User not found! Email: {email}");
            return result;
        }

        var loans = user.Loans;
        if (loans == null)
        {
            // No loans
            Console.WriteLine($"[UserService::GetUserLoansByEmailAsync] Loans are empty! Email: {email}");
            return result;
        }

        Parallel.ForEach(loans, loan =>
        {
            result.Add(new UserLoanDTO()
            { 
                LoanId = loan.Id,
                BookId = loan.BookId,
                Extensions = loan.Extensions,
                StartDate = loan.StartDate,
                ExpectedEndDate = loan.ExpectedEndDate,
                ReturnDate = loan.ReturnDate,
            });
        });

        return result;
    }

    public async Task<List<UserLoanDTO>> GetUserSelectedLoansByEmailAsync(string email, List<UserLoanDTO> userLoanDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return null;
        }
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Loans).FirstOrDefaultAsync();
        
        var result = new List<UserLoanDTO>();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::GetUserSelectedLoansAsync] User not found! Email: {email}");
            return result;
        }

        var loans = user.Loans;
        if (loans == null)
        {
            // No loans
            Console.WriteLine($"[UserService::GetUserSelectedLoansAsync] Loans are empty! Email: {email}");
            return result;
        }
        
        var convertId = userLoanDto.ConvertAll(o => o.LoanId);

        var lookupLoans = loans.Where(l => convertId.Contains(l.Id)).ToList();

        Parallel.ForEach(lookupLoans, loan =>
        {
            result.Add(new UserLoanDTO()
            { 
                LoanId = loan.Id,
                BookId = loan.BookId,
                Extensions = loan.Extensions,
                StartDate = loan.StartDate,
                ExpectedEndDate = loan.ExpectedEndDate,
                ReturnDate = loan.ReturnDate,
            });
        });

        return result;
    }

    public async Task<bool> UpdateUserContactInformationAsync(UserModifyContactDTO userModifyContactDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(userModifyContactDto.Email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        var user = await _dbContext.Users.Where(u => u.Email == userModifyContactDto.Email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserContactInformationAsync] User not found! Email: {userModifyContactDto.Email}");
            return false;
        }

        // Modify user contact info
        Console.WriteLine($"[UserService::UpdateUserContactInformationAsync] Updating user contact information... Email: {userModifyContactDto.Email}");
        
        _dbContext.Users.Update(user);
        
        user.FirstName = userModifyContactDto.FirstName;
        user.LastName = userModifyContactDto.LastName;
        user.Phone = userModifyContactDto.Phone;
        user.Address = userModifyContactDto.Address;
        
        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserContactInformationAsync] Updated user contact information. Email: {userModifyContactDto.Email}");

        return true;
    }

    public async Task<bool> UpdateUserLoginAsync(UserModifyLoginDTO userModifyLoginDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(userModifyLoginDto.OldEmail);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        var user = await _dbContext.Users.Where(u => u.Email == userModifyLoginDto.OldEmail).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserEmailAsync] User not found! Email: {userModifyLoginDto.OldEmail}");
            return false;
        }
        
        _dbContext.Users.Update(user);

        user.Email = userModifyLoginDto.NewEmail;
            
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"[UserService::UpdateUserEmailAsync] Updated user email. Email: {userModifyLoginDto.OldEmail} -> {userModifyLoginDto.NewEmail}");
        return true;
    }

    public async Task<bool> UpdateUserReservationsAsync(string email, List<UserReservationDTO> userModifyReservationDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }

        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(email)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Reservations).FirstOrDefaultAsync();
        
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserReservationsAsync] User not found! Email: {email}");
            return false;
        }
        
        if (userModifyReservationDto.Count < 1)
        {
            // If nothing is going to be added, do not continue
            Console.WriteLine($"[UserService::UpdateUserReservationsAsync] DTO was an empty list! Email: {email}");
            return false;
        }

        if (user.Reservations == null)
        {
            // if the user has a null list, create an empty list for it
            user.Reservations = new List<Reservation>();
        }
        
        _dbContext.Users.Update(user);

        Parallel.ForEach(userModifyReservationDto, userReservationDto =>
        {
            user.Reservations.Add(new Reservation()
            {
                BookId = userReservationDto.BookId ?? -1,
                UserEmail = user.Email,
                IsAccepted = userReservationDto.IsAccepted ?? false,
                ReservationDate = userReservationDto.ReservationDate ?? DateTime.UtcNow,
                ExpectedStart = userReservationDto.ExpectedStart ?? DateTime.UtcNow,
                ExpectedEnd = userReservationDto.ExpectedEnd ?? DateTime.UtcNow,
            });
        });

        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserReservationsAsync] Modified user reservations! Email: {email}");

        return true;
    }

    public async Task<bool> UpdateUserLoansAsync(string email, List<UserLoanDTO> userModifyLoanDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        
        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(email)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Loans).FirstOrDefaultAsync();
        
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserLoansAsync] User not found! Email: {email}");
            return false;
        }
        
        if (userModifyLoanDto.Count < 1)
        {
            // If nothing is going to be added, do not continue
            Console.WriteLine($"[UserService::UpdateUserLoansAsync] DTO was an empty list! Email: {email}");
            return false;
        }

        if (user.Loans == null)
        {
            // if the user has a null list, create an empty list for it
            user.Loans = new List<Loan>();
        }
        
        _dbContext.Users.Update(user);

        Parallel.ForEach(userModifyLoanDto, userLoanDto =>
        {
            user.Loans.Add(new Loan()
            {
                BookId = userLoanDto.BookId ?? -1,
                UserEmail = user.Email,
                Extensions = userLoanDto.Extensions ?? new Loan().Extensions, // use the default extention value of loans
                StartDate = userLoanDto.StartDate ?? DateTime.UtcNow,
                ExpectedEndDate = userLoanDto.ExpectedEndDate ?? DateTime.UtcNow,
                ReturnDate = userLoanDto.ReturnDate
            });
        });

        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserLoansAsync] Modified user loans! Email: {email}");

        return true;
    }

    public async Task<bool> UpdateUserPrivilegeAsync(UserModifyPrivilegeDTO userModifyPrivilegeDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(userModifyPrivilegeDto.RequesterEmail);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        
        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(userModifyPrivilegeDto.RequesterEmail)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var changeToken = userModifyPrivilegeDto.ChangeToken;
        //TODO: Check if change token is valid
        if (changeToken == "")
        {
            // Token is invalid
            Console.WriteLine($"[UserService::UpdateUserPrivilegeAsync] Invalid Token! Token: {userModifyPrivilegeDto.ChangeToken}");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == userModifyPrivilegeDto.UserEmail).FirstOrDefaultAsync();
        var requester = await _dbContext.Users.Where(u => u.Email == userModifyPrivilegeDto.RequesterEmail).FirstOrDefaultAsync();

        if (user == null || requester == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserPrivilegeAsync] User not found! Email: {userModifyPrivilegeDto.UserEmail} - {userModifyPrivilegeDto.RequesterEmail}");
            return false;
        }

        if (user.Email == requester.Email)
        {
            // cant change self permission
            Console.WriteLine($"[UserService::UpdateUserPrivilegeAsync] Cant change self permissions! Email: {user.Email}");
            return false;
        }

        if (requester.Privilege > userModifyPrivilegeDto.NewPrivilege)
        {
            // cant elevate privileges above self for others (librarian cant give anyone admin, only librarian)
            Console.WriteLine($"[UserService::UpdateUserPrivilegeAsync] Cant elevate users privilege above self! Email: {userModifyPrivilegeDto.UserEmail} - {userModifyPrivilegeDto.RequesterEmail}");
            return false;
        }

        _dbContext.Users.Update(user);

        user.Privilege = userModifyPrivilegeDto.NewPrivilege; // Change user privilege
        
        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserPrivilegeAsync] User privilege has been modified! Email: {userModifyPrivilegeDto.UserEmail}, Privilege: {userModifyPrivilegeDto.NewPrivilege}");
        return true;
    }

    public async Task<bool> RemoveUserAsync(string email)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        
        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(email)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::RemoveUserAsync] User not found! Email: {email}");
            return false;
        }
        
        _dbContext.Users.Remove(user);

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveUserReservationAsync(string email, List<UserReservationDTO> userModifyReservationDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        
        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(email)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Reservations).FirstOrDefaultAsync();
        
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::RemoveUserReservationAsync] User not found! Email: {email}");
            return false;
        }
        
        if (userModifyReservationDto.Count < 1)
        {
            // If nothing is going to be added, do not continue
            Console.WriteLine($"[UserService::RemoveUserReservationAsync] DTO was an empty list! Email: {email}");
            return false;
        }

        if (user.Reservations == null)
        {
            // if the user has a null list, we have nothing to remove
            Console.WriteLine($"[UserService::RemoveUserReservationAsync] User has an empty list! Email: {email}");
            return false;
        }
        
        _dbContext.Users.Update(user);

        Parallel.ForEach(userModifyReservationDto, userReservationDto =>
        {
            if (userReservationDto.ReservationId == null)
            {
                Console.WriteLine($"[UserService::RemoveUserReservationAsync] Got an empty reservation ID! Email: {email}");
                return;
            }

            var reservation = Task.Run(() => _dbContext.Reservations.Where(r => r.Id == userReservationDto.ReservationId).FirstOrDefaultAsync()).Result;
            if (reservation == null)
            {
                Console.WriteLine($"[UserService::RemoveUserReservationAsync] Reservation not found! Email: {email}");
                return;
            }
            _dbContext.Reservations.Remove(reservation);
        });

        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserReservationsAsync] Modified user reservations! Email: {email}");

        return true;
    }

    public async Task<bool> RemoveUserLoansAsync(string email, List<UserLoanDTO> userModifyLoanDto)
    {
        var authenticated = await GetUserAuthenticatedAsync(email);
        if (!authenticated)
        {
            Console.WriteLine("User not authenticated!");
            return false;
        }
        
        var hasPermission = ((await GetUserPrivilegeLevelByEmailAsync(email)).Privilege &
                             (PrivilegeLevel.Admin | PrivilegeLevel.Librarian)) > 0;
        if (!hasPermission)
        {
            Console.WriteLine("Insufficient privileges!");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == email)
            .Include(user => user.Loans).FirstOrDefaultAsync();
        
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::RemoveUserLoansAsync] User not found! Email: {email}");
            return false;
        }
        
        if (userModifyLoanDto.Count < 1)
        {
            // If nothing is going to be added, do not continue
            Console.WriteLine($"[UserService::RemoveUserLoansAsync] DTO was an empty list! Email: {email}");
            return false;
        }

        if (user.Loans == null)
        {
            // if the user has a null list, we have nothing to remove
            Console.WriteLine($"[UserService::RemoveUserLoansAsync] User has an empty list! Email: {email}");
            return false;
        }
        
        _dbContext.Users.Update(user);

        Parallel.ForEach(userModifyLoanDto, userLoanDto =>
        {
            if (userLoanDto.LoanId == null)
            {
                Console.WriteLine($"[UserService::RemoveUserLoansAsync] Got an empty loan ID! Email: {email}");
                return;
            }

            var loan = Task.Run(() => _dbContext.Loans.Where(l => l.Id == userLoanDto.LoanId).FirstOrDefaultAsync()).Result;
            if (loan == null)
            {
                Console.WriteLine($"[UserService::RemoveUserLoansAsync] Loan not found! Email: {email}");
                return;
            }
            _dbContext.Loans.Remove(loan);
        });

        await _dbContext.SaveChangesAsync();
        
        Console.WriteLine($"[UserService::UpdateUserReservationsAsync] Modified user reservations! Email: {email}");

        return true;
    }

    public async Task<UserLoginTokenDTO> PostAutenticationAsync(UserLoginValuesDTO userLoginValuesDto)
    {
        var user = await _dbContext.Users.Where(u => u.Email == userLoginValuesDto.Email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::PostAutenticationAsync] User not found! Email: {userLoginValuesDto.Email}");
            return new UserLoginTokenDTO()
            {
                AuthToken = null
            };
        }
        if (user.PasswordHash != userLoginValuesDto.PasswordHash)
        {
            // Invalid password
            Console.WriteLine($"[UserService::PostAutenticationAsync] User password does not match! Email: {userLoginValuesDto.Email}");
            return new UserLoginTokenDTO()
            {
                AuthToken = null
            };
        }

        Console.WriteLine($"[UserService::PostAutenticationAsync] User authenticated! Email: {userLoginValuesDto.Email}");
        return new UserLoginTokenDTO()
        {
            AuthToken = "AuthToken"
        };
    }

    public async Task<bool> PostUserCreateAsync(UserLoginValuesDTO userLoginValuesDto)
    {
        var user = await _dbContext.Users.Where(u => u.Email == userLoginValuesDto.Email).FirstOrDefaultAsync();
        if (user != null)
        {
            // User creation is invalid
            Console.WriteLine($"[UserService::PostUserCreateAsync] User already exists! Email: {userLoginValuesDto.Email}");
            return false;
        }
        
        _dbContext.Users.Add(new User()
        {
            Email = userLoginValuesDto.Email,
            PasswordHash = userLoginValuesDto.PasswordHash,
            Privilege = PrivilegeLevel.Registered // We should only give this role, as privilege changing will be done by an admin, or librarian
        });
        
        await _dbContext.SaveChangesAsync();

        return true;
    }
}