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
    /// Get the users contact information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<UserGetContactDTO> GetUserContactInformationByEmailAsync(string email);
    /// <summary>
    /// Get the users privilige information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<UserGetPriviligeLevelDTO> GetUserPrivilegeLevelByEmailAsync(string email);
    /// <summary>
    /// Get the users reservation information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserReservationDTO>> GetUserReservationsByEmailAsync(string email);
    /// <summary>
    /// Get the users loan information based on its email address
    /// </summary>
    /// <param name="email">The email address of the user</param>
    /// <returns>A DTO object which contains the requested information</returns>
    Task<List<UserLoanDTO>> GetUserLoansByEmailAsync(string email);
    
    // Modify   ------------------------------------------------------------------------------------------------------------------------
    
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
    /// <param name="userModifyEmailDto">The DTO from which the user email will be changed to</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserEmailAsync(UserModifyEmailDTO userModifyEmailDto);
    
    /// <summary>
    /// Add the reservations from the given DTO to the user
    /// </summary>
    /// <param name="userModifyReservationDto">The DTO from which the new reservations are from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserReservationsAsync(List<UserReservationDTO> userModifyReservationDto);
    
    /// <summary>
    /// Add the loans from the given DTO to the user
    /// </summary>
    /// <param name="userModifyLoanDto">The DTO from which the new loans are from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserLoansAsync(List<UserLoanDTO> userModifyLoanDto);
    
    /// <summary>
    /// Changes the users privilige level
    /// This should only be allowed by an administrator!!!
    /// </summary>
    /// <param name="userModifyPriviligeDto">The DTO from which the new privilige is from</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> UpdateUserPriviligeAsync(UserModifyPriviligeDTO userModifyPriviligeDto);
    
    // Delete   ------------------------------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Remove the reservations from the given DTO from the user
    /// </summary>
    /// <param name="userModifyReservationDto">The DTO from which we are removing from the user</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> RemoveUserReservationAsync(List<UserReservationDTO> userModifyReservationDto);
    
    /// <summary>
    /// Remove the loans from the given DTO from the user
    /// </summary>
    /// <param name="userModifyLoanDto">The DTO from which we are removing from the user</param>
    /// <returns>The result of the requested action (true for success)</returns>
    Task<bool> RemoveUserLoansAsync(List<UserLoanDTO> userModifyLoanDto);
    
    // Post     ------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Authenticate the given user from the DTO
    /// </summary>
    /// <param name="userLoginValuesDto">The DTO contains the users data, from the frontend</param>
    /// <returns>A DTO with their auth token</returns>
    Task<UserLoginTokenDTO> PostAutenticationAsync(UserLoginValuesDTO userLoginValuesDto);
}

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserGetContactDTO> GetUserContactInformationByEmailAsync(string email)
    {
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
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

    public async Task<UserGetPriviligeLevelDTO> GetUserPrivilegeLevelByEmailAsync(string email)
    {
        var user = await _dbContext.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::GetUserPrivilegeLevelByEmailAsync] User not found! Email: {email}");
            return new UserGetPriviligeLevelDTO()
            {
                Privilige = PrivilegeLevel.UnRegistered,
                PriviligeString = PrivilegeLevel.UnRegistered.ToString()
            };
        }
        // Otherwise, give back real user data
        return new UserGetPriviligeLevelDTO()
        {
            Privilige = user.Privilege,
            PriviligeString = user.Privilege.ToString()
        };
    }

    public async Task<List<UserReservationDTO>> GetUserReservationsByEmailAsync(string email)
    {
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
        foreach (var reservation in reservations)
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
        }

        return result;
    }

    public async Task<List<UserLoanDTO>> GetUserLoansByEmailAsync(string email)
    {
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
        foreach (var loan in loans)
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
        }

        return result;
    }

    public async Task<bool> UpdateUserContactInformationAsync(UserModifyContactDTO userModifyContactDto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserEmailAsync(UserModifyEmailDTO userModifyEmailDto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserReservationsAsync(List<UserReservationDTO> userModifyReservationDto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserLoansAsync(List<UserLoanDTO> userModifyLoanDto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateUserPriviligeAsync(UserModifyPriviligeDTO userModifyPriviligeDto)
    {
        var changeToken = userModifyPriviligeDto.ChangeToken;
        //TODO: Check if change token is valid
        Console.WriteLine("[UserService::UpdateUserPriviligeAsync] TODO!!! Check change token, disallow unauthenticated requests!!!!");
        if (changeToken == "")
        {
            // Token is invalid
            Console.WriteLine($"[UserService::UpdateUserPriviligeAsync] Invalid Token! Token: {userModifyPriviligeDto.ChangeToken}");
            return false;
        }
        
        var user = await _dbContext.Users.Where(u => u.Email == userModifyPriviligeDto.Email).FirstOrDefaultAsync();
        if (user == null)
        {
            // User is invalid
            Console.WriteLine($"[UserService::UpdateUserPriviligeAsync] User not found! Email: {userModifyPriviligeDto.Email}");
            return false;
        }

        user.Privilege = userModifyPriviligeDto.NewPrivilege; // Change user privilige
        Console.WriteLine($"[UserService::UpdateUserPriviligeAsync] User privilige has been modified! Email: {userModifyPriviligeDto.Email}, Privilige: {userModifyPriviligeDto.NewPrivilege}");
        return true;
    }

    public async Task<bool> RemoveUserReservationAsync(List<UserReservationDTO> userModifyReservationDto)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveUserLoansAsync(List<UserLoanDTO> userModifyLoanDto)
    {
        throw new NotImplementedException();
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
            };;
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
}