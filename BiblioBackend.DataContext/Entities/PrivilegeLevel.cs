using System.Collections.Generic;

namespace BiblioBackend.DataContext.Entities
{
    public enum PrivilegeLevel
    {
        Admin,
        Librarian,
        Registered,
        UnRegistered
    }

    public static class PrivilegeLevelExtensions
    {
        private static readonly Dictionary<PrivilegeLevel, string> PrivilegeLevelStrings = new()
        {
            { PrivilegeLevel.Admin, "Administrator" },
            { PrivilegeLevel.Librarian, "Librarian" },
            { PrivilegeLevel.Registered, "Registered User" },
            { PrivilegeLevel.UnRegistered, "Unregistered User" }
        };

        public static string ToFriendlyString(this PrivilegeLevel privilege)
        {
            return PrivilegeLevelStrings.TryGetValue(privilege, out var value) ? value : privilege.ToString();
        }
    }
}