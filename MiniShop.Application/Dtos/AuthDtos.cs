namespace MiniShop.Application.Dtos;

/// <summary>Registracija kupca. E-adresa je i korisnicko ime.</summary>
public record RegisterDto(string Email, string Password, string? FullName);

/// <summary>Prijava e-adresom i lozinkom.</summary>
public record LoginDto(string Email, string Password);

// nedelja 10: AuthResponse(AccessToken, ExpiresAt, RefreshToken), RefreshDto(RefreshToken)