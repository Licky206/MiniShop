namespace MiniShop.Api.Auth;

/// <summary>Rezultat uspesne rotacije: vlasnik i novi sirovi refresh token.</summary>
public sealed record RotatedRefreshToken(string UserId, string RefreshToken);

/// <summary>
/// Izdavanje, rotacija i opoziv refresh tokena. U bazi je samo SHA-256 hes;
/// sirov token postoji samo u odgovoru klijentu.
/// </summary>
public sealed class RefreshTokenService(
    AppDbContext db,
    IOptions<JwtOptions> options,
    ILogger<RefreshTokenService> log)
{
    /// <summary>Novi refresh token za korisnika (prijava). Vraca sirov token.</summary>
    public async Task<string> IssueAsync(string userId, CancellationToken ct)
    {
        var (raw, entity) = Create(userId);
        db.RefreshTokens.Add(entity);
        await db.SaveChangesAsync(ct);
        return raw;
    }

    /// <summary>
    /// Menja aktivan refresh token za novi. Vraca null za nepoznat, istekao ili
    /// opozvan token; ponovna upotreba iskoriscenog tokena opoziva celu porodicu.
    /// </summary>
    public async Task<RotatedRefreshToken?> RotateAsync(string rawToken, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var old = await db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (old is null)
            return null;                                   // nepoznat token

        if (old.UsedAt is not null)
        {
            // vec rotiran token je stigao ponovo: postoji kopija u tudjim rukama
            log.LogWarning("Ponovna upotreba refresh tokena {TokenId} korisnika {UserId}",
                old.Id, old.UserId);
            await RevokeFamilyAsync(old.UserId, ct);
            return null;
        }

        if (!old.IsActive)
            return null;                                   // istekao ili opozvan

        // atomsko preuzimanje: UPDATE sa uslovom UsedAt IS NULL uspeva samo jednom,
        // pa dva istovremena zahteva sa istim tokenom ne mogu oba da prodju
        var now = DateTime.UtcNow;
        var claimed = await db.RefreshTokens
            .Where(t => t.Id == old.Id && t.UsedAt == null && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, now), ct);

        if (claimed == 0)
        {
            log.LogWarning("Istovremena upotreba refresh tokena {TokenId} korisnika {UserId}",
                old.Id, old.UserId);
            await RevokeFamilyAsync(old.UserId, ct);
            return null;
        }

        var (raw, next) = Create(old.UserId);
        db.RefreshTokens.Add(next);
        await db.SaveChangesAsync(ct);

        // trag rotacije: stari zapis pokazuje na novi (Id je poznat tek posle INSERT-a)
        await db.RefreshTokens
            .Where(t => t.Id == old.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ReplacedById, next.Id), ct);

        return new RotatedRefreshToken(old.UserId, raw);
    }

    /// <summary>Logout: opoziva jedan refresh token. Nepoznat token se tiho ignorise.</summary>
    public async Task RevokeAsync(string rawToken, CancellationToken ct)
    {
        var hash = Hash(rawToken);
        var now = DateTime.UtcNow;
        await db.RefreshTokens
            .Where(t => t.TokenHash == hash && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>
    /// Opoziva sve jos vazece refresh tokene korisnika: ponovna upotreba,
    /// promena lozinke, blokada naloga. Vraca broj opozvanih zapisa.
    /// </summary>
    public Task<int> RevokeFamilyAsync(string userId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        return db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
    }

    /// <summary>SHA-256 sirovog tokena, Base64 (44 znaka).</summary>
    public static string Hash(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    private (string Raw, RefreshToken Entity) Create(string userId)
    {
        // 32 kriptografski slucajna bajta; Base64Url je bezbedan i za URL i za cookie
        var raw = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = Hash(raw),
            ExpiresAt = DateTime.UtcNow.AddDays(options.Value.RefreshTokenDays)
        };
        return (raw, entity);
    }
}