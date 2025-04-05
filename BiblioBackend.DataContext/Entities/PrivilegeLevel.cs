namespace BiblioBackend.DataContext.Entities;

[Flags]
public enum PrivilegeLevel
{
    Admin = 0b0001,
    Librarian = 0b0010,
    Registered = 0b0100,
    UnRegistered = 0b1000
}

public static class PrivilegeLevelStringify
{
    public static Dictionary<string, string> PrivilegeLevelStrings =>
        new()
        {
            {PrivilegeLevel.Admin.ToString(), "Adminisztrátor"},
            {PrivilegeLevel.Librarian.ToString(), "Könyvtáros"},
            {PrivilegeLevel.Registered.ToString(), "Regisztrált Felhasználó"},
            {PrivilegeLevel.UnRegistered.ToString(), "Nem Regisztrált Felhasználó"},
        };
}