namespace MiniShop.Domain.Entities;

/// <summary>Zapis o izdatom refresh tokenu. Sirov token se nikada ne cuva.</summary>
public class RefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string TokenHash { get; set; } = "";      // SHA-256 (Base64), nikada sirov token
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }            // rotiran: zamenio ga je novi
    public DateTime? RevokedAt { get; set; }         // opozvan: logout ili napad
    public int? ReplacedById { get; set; }           // Id tokena koji ga je zamenio

    public bool IsActive => UsedAt is null && RevokedAt is null
                            && ExpiresAt > DateTime.UtcNow;
}