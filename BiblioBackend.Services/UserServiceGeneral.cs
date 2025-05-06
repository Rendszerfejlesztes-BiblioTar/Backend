using BiblioBackend.DataContext.Entities;
using BiblioBackend.Services;

namespace BiblioBackend
{
    public static class UserServiceGeneral
    {
        /// <summary>
        /// Checks if a user with the given email exists in the database.
        /// </summary>
        /// <param name="userService">The user service instance.</param>
        /// <param name="email">The user's email.</param>
        /// <returns>True if the user exists; otherwise, false.</returns>
        public static async Task<bool> CheckIsUserExistsAsync(IUserService userService, string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return await userService.GetUserIsExistsAsync(email);
        }

        /// <summary>
        /// Checks if a user with the given email is authenticated via JWT.
        /// </summary>
        /// <param name="userService">The user service instance.</param>
        /// <param name="email">The user's email.</param>
        /// <returns>True if the user is authenticated; otherwise, false.</returns>
        public static async Task<bool> CheckIsUserAuthenticatedAsync(IUserService userService, string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            return await userService.GetUserAuthenticatedAsync(email);
        }

        /// <summary>
        /// Checks if a user has one of the required privilege levels.
        /// </summary>
        /// <param name="userService">The user service instance.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="neededPrivileges">The required privilege levels.</param>
        /// <returns>True if the user has one of the required privileges; otherwise, false.</returns>
        public static async Task<bool> CheckIsUserPermittedAsync(IUserService userService, string email, params PrivilegeLevel[] neededPrivileges)
        {
            if (string.IsNullOrEmpty(email) || neededPrivileges.Length == 0)
                return false;

            try
            {
                var actualPrivilege = await userService.GetUserPrivilegeLevelByEmailAsync(email);
                return neededPrivileges.Contains(actualPrivilege);
            }
            catch (InvalidOperationException)
            {
                return false; // User not found
            }
        }
    }
}