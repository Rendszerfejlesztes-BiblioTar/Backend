using BiblioBackend.DataContext.Context;
using BiblioBackend.DataContext.Dtos;
using BiblioBackend.DataContext.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System;
using System.Threading.Tasks;
using BiblioBackend.BiblioBackend.DataContext.Dtos.User;
using BiblioBackend.DataContext.Dtos.User.Post;
using BiblioBackend.DataContext.Dtos.User;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace BiblioBackend.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterUserAsync(UserLoginValuesDto userDto);
        Task<DefaultAdminDto> CreateDefaultAdmin();
        Task<UserLoginDto> AuthenticateUserAsync(UserLoginValuesDto userDto);
        Task<UserLoginDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
        Task<bool> RevokeTokenAsync(string email);
        Task<UserDto> GetUserAsync(string email);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserContactAsync(UserModifyContactDto userDto);
        Task<bool> UpdateUserLoginAsync(UserModifyLoginDto userDto);
        Task<bool> UpdateUserPrivilegeAsync(UserModifyPrivilegeDto userDto);
        Task<bool> DeleteUserAsync(string email);
        Task<bool> GetUserIsExistsAsync(string email);
        Task<bool> GetUserAuthenticatedAsync(string email);
        Task<PrivilegeLevel> GetUserPrivilegeLevelByEmailAsync(string email);
    }
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext dbContext, IConfiguration configuration, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<UserDto> RegisterUserAsync(UserLoginValuesDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
            {
                _logger.LogWarning("Registration failed: Invalid input data");
                throw new ArgumentException("Email and password are required.");
            }

            _logger.LogInformation("Attempting to register user: {Email}", userDto.Email);

            if (await _dbContext.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", userDto.Email);
                throw new InvalidOperationException("Email already exists.");
            }

            var user = new User
            {
                Email = userDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password),
                Privilege = PrivilegeLevel.Registered
            };

            _dbContext.Users.Add(user);
            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("User registered successfully: {Email}", userDto.Email);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists (DB constraint)", userDto.Email);
                throw new InvalidOperationException("Email already exists.");
            }

            return new UserDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                Address = user.Address,
                Privilege = user.Privilege
            };
        }

        public async Task<DefaultAdminDto?> CreateDefaultAdmin()
        {
            const string defaultEmail = "admin@example.com";

            var exists = _dbContext.Users.Any(u => u.Email == defaultEmail);
            if (exists)
            {
                _logger.LogInformation("No default admin was created, already exists!");
                return null;
            }
            
            const string defaultAddress = "8000 Veszprém, József Attila Utca 1";
            const string defaultFirstName = "Kovács";
            const string defaultLastName = "István";
            const string defaultPhone = "06123456789";
            const string defaultPass = "Admin123";
            
            _dbContext.Users.Add(new User()
            {
                Email = defaultEmail,
                Address = defaultAddress,
                FirstName = defaultFirstName,
                LastName = defaultLastName,
                Phone = defaultPhone,
                Privilege = PrivilegeLevel.Admin,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPass),
            });
            _logger.LogInformation("Created the default admin user.");
            await _dbContext.SaveChangesAsync();
            return new DefaultAdminDto
            {
                Email = defaultEmail,
                Password = defaultPass
            };
        }

        public async Task<UserLoginDto> AuthenticateUserAsync(UserLoginValuesDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Email) || string.IsNullOrEmpty(userDto.Password))
            {
                _logger.LogWarning("Authentication failed: Invalid input data");
                throw new ArgumentException("Email and password are required.");
            }

            _logger.LogInformation("Attempting to authenticate user: {Email}", userDto.Email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed: Invalid email or password for {Email}", userDto.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]));
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User authenticated successfully: {Email}", userDto.Email);

            return new UserLoginDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"])),
                User = new UserDto()
                {
                    Address = user.Address,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Privilege = user.Privilege
                }
            };
        }

        public async Task<UserLoginDto> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            if (refreshTokenDto == null || string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                _logger.LogWarning("Token refresh failed: Invalid refresh token");
                throw new ArgumentException("Refresh token is required.");
            }

            _logger.LogInformation("Attempting to refresh token");

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshTokenDto.RefreshToken);
            if (user == null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
                throw new SecurityTokenException("Invalid or expired refresh token.");
            }

            var token = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenLifetimeDays"]));
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Token refreshed successfully for user: {Email}", user.Email);

            return new UserLoginDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"]))
            };
        }

        public async Task<bool> RevokeTokenAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Token revocation failed: Email is required");
                throw new ArgumentException("Email is required.");
            }

            _logger.LogInformation("Attempting to revoke token for user: {Email}", email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Token revocation failed: User {Email} not found", email);
                return false;
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Token revoked successfully for user: {Email}", email);
            return true;
        }

        public async Task<UserDto> GetUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User retrieval failed: Email is required");
                throw new ArgumentException("Email is required.");
            }

            _logger.LogInformation("Retrieving user: {Email}", email);

            var user = await _dbContext.Users
                .Where(u => u.Email == email)
                .Select(u => new UserDto
                {
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Phone = u.Phone,
                    Address = u.Address,
                    Privilege = u.Privilege
                })
                .FirstOrDefaultAsync();
            if (user == null)
            {
                _logger.LogWarning("User retrieval failed: User {Email} not found", email);
                throw new KeyNotFoundException($"User with email {email} not found.");
            }

            _logger.LogInformation("User retrieved successfully: {Email}", email);
            return user;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _dbContext.Users.ToListAsync();
            return users.ConvertAll(c => new UserDto()
            {
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Address = c.Address,
                Privilege = c.Privilege
            });
        }

        public async Task<bool> UpdateUserContactAsync(UserModifyContactDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.Email))
            {
                _logger.LogWarning("Contact update failed: Invalid input data");
                throw new ArgumentException("Email is required.");
            }

            _logger.LogInformation("Attempting to update contact info for user: {Email}", userDto.Email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Contact update failed: User {Email} not found", userDto.Email);
                throw new KeyNotFoundException($"User with email {userDto.Email} not found.");
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Phone = userDto.Phone;
            user.Address = userDto.Address;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Contact info updated successfully for user: {Email}", userDto.Email);
            return true;
        }

        public async Task<bool> UpdateUserLoginAsync(UserModifyLoginDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.OldEmail) || string.IsNullOrEmpty(userDto.NewEmail))
            {
                _logger.LogWarning("Login update failed: Invalid input data");
                throw new ArgumentException("Old and new email are required.");
            }

            _logger.LogInformation("Attempting to update login info for user: {OldEmail}", userDto.OldEmail);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.OldEmail);
            if (user == null)
            {
                _logger.LogWarning("Login update failed: User {OldEmail} not found", userDto.OldEmail);
                throw new KeyNotFoundException($"User with email {userDto.OldEmail} not found.");
            }

            if (await _dbContext.Users.AnyAsync(u => u.Email == userDto.NewEmail && u.Email != userDto.OldEmail))
            {
                _logger.LogWarning("Login update failed: New email {NewEmail} already in use", userDto.NewEmail);
                throw new InvalidOperationException("New email is already in use.");
            }

            user.Email = userDto.NewEmail;
            if (!string.IsNullOrEmpty(userDto.NewPassword))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.NewPassword);

            try
            {
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Login info updated successfully for user: {NewEmail}", userDto.NewEmail);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Users_Email") == true)
            {
                _logger.LogWarning("Login update failed: New email {NewEmail} already in use (DB constraint)", userDto.NewEmail);
                throw new InvalidOperationException("New email is already in use.");
            }

            return true;
        }

        public async Task<bool> UpdateUserPrivilegeAsync(UserModifyPrivilegeDto userDto)
        {
            if (userDto == null || string.IsNullOrEmpty(userDto.UserEmail) || string.IsNullOrEmpty(userDto.RequesterEmail))
            {
                _logger.LogWarning("Privilege update failed: Invalid input data");
                throw new ArgumentException("User email and requester email are required.");
            }

            _logger.LogInformation("Attempting to update privilege for user: {UserEmail} by requester: {RequesterEmail}", userDto.UserEmail, userDto.RequesterEmail);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.UserEmail);
            var requester = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == userDto.RequesterEmail);
            if (user == null || requester == null)
            {
                _logger.LogWarning("Privilege update failed: User {UserEmail} or requester {RequesterEmail} not found", userDto.UserEmail, userDto.RequesterEmail);
                return false;
            }

            if (user.Email == requester.Email && user.Privilege == PrivilegeLevel.Registered && userDto.NewPrivilege == PrivilegeLevel.Admin)
            {
                _logger.LogInformation("Allowing self-privilege change for initial admin creation: {UserEmail}", userDto.UserEmail);
            }
            else if (user.Email == requester.Email)
            {
                _logger.LogWarning("Privilege update failed: Cannot change own privileges for {UserEmail}", userDto.UserEmail);
                throw new InvalidOperationException("Cannot change own privileges.");
            }
            else if (requester.Privilege > userDto.NewPrivilege)
            {
                _logger.LogWarning("Privilege update failed: Requester {RequesterEmail} cannot elevate privileges above {RequesterPrivilege}", userDto.RequesterEmail, requester.Privilege.ToFriendlyString());
                throw new InvalidOperationException("Cannot elevate privileges above requester's level.");
            }

            user.Privilege = userDto.NewPrivilege;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Privilege updated successfully for user: {UserEmail} to {NewPrivilege}", userDto.UserEmail, userDto.NewPrivilege.ToFriendlyString());
            return true;
        }

        public async Task<bool> DeleteUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User deletion failed: Email is required");
                throw new ArgumentException("Email is required.");
            }

            _logger.LogInformation("Attempting to delete user: {Email}", email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("User deletion failed: User {Email} not found", email);
                return false;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("User deleted successfully: {Email}", email);
            return true;
        }

        public async Task<bool> GetUserIsExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("User existence check failed: Email is required");
                return false;
            }

            _logger.LogInformation("Checking if user exists: {Email}", email);
            var exists = await _dbContext.Users.AnyAsync(u => u.Email == email);
            _logger.LogInformation("User existence check result for {Email}: {Exists}", email, exists);
            return exists;
        }

        public async Task<bool> GetUserAuthenticatedAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Authentication check failed: Email is required");
                return false;
            }

            _logger.LogInformation("Checking if user is authenticated: {Email}", email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Authentication check failed: User {Email} not found", email);
                return false;
            }

            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal == null)
            {
                _logger.LogWarning("Authentication check failed: No user context available for {Email}", email);
                return false;
            }

            var jwtEmail = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
            var isAuthenticated = jwtEmail == email;
            _logger.LogInformation("Authentication check result for {Email}: {IsAuthenticated}", email, isAuthenticated);
            return isAuthenticated;
        }

        public async Task<PrivilegeLevel> GetUserPrivilegeLevelByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Privilege retrieval failed: Email is required");
                throw new ArgumentException("Email is required.");
            }

            _logger.LogInformation("Retrieving privilege for user: {Email}", email);

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                _logger.LogWarning("Privilege retrieval failed: User {Email} not found", email);
                throw new KeyNotFoundException($"User with email {email} not found.");
            }

            _logger.LogInformation("Privilege retrieved for user {Email}: {Privilege}", email, user.Privilege.ToFriendlyString());
            return user.Privilege;
        }

        private string GenerateJwtToken(User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Email))
            {
                _logger.LogError("JWT generation failed: Invalid user data");
                throw new ArgumentException("User data is required.");
            }

            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Privilege.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenLifetimeMinutes"])),
                signingCredentials: creds);

            _logger.LogInformation("JWT generated successfully for user: {Email}", user.Email);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                var refreshToken = Convert.ToBase64String(randomNumber);
                _logger.LogInformation("Refresh token generated");
                return refreshToken;
            }
        }
    }
}