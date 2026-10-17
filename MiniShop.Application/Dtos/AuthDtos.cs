namespace MiniShop.Application.Dtos;

public record RegisterDto(string Email, string Password, string? FullName);

public record LoginDto(string Email, string Password);

/// <summary>Odgovor prijave i obnove: access token, njegov istek i novi refresh token.</summary>
public record AuthResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);

public record RefreshDto(string RefreshToken);