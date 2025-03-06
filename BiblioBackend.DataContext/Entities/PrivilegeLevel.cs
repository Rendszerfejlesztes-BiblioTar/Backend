namespace BiblioBackend.DataContext.Entities;

public enum PrivilegeLevel
{
    // Minden rangnak van egy privilige bit-je
    Admin = 0b0001,
    Librarian = 0b0010,
    Registered = 0b0100,
    UnRegistered = 0b1000
}