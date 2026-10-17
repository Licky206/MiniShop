namespace MiniShop.Domain.Entities;

/// <summary>
/// Korisnik MiniShop-a. Id, UserName, Email, PasswordHash, lockout i 2FA polja
/// dolaze iz IdentityUser; Id je string, zato je Order.UserId string.
/// </summary>
public class AppUser : IdentityUser
{
    public string? FullName { get; set; }
}