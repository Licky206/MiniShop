namespace MiniShop.Api.Auth;

/// <summary>
/// Izdaje potpisane access tokene (HS256). Token pise JwtSecurityTokenHandler;
/// validaciju radi JwtBearer handler (AddJwtBearer u Program.cs).
/// </summary>
public sealed class TokenService(IOptions<JwtOptions> options)
{
    public (string Token, DateTime ExpiresAt) Create(AppUser user, IEnumerable<string> roles)
    {
        var o = options.Value;
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(o.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            // sub -> posle validacije ClaimTypes.NameIdentifier (inbound mapiranje)
            new(JwtRegisteredClaimNames.Sub, user.Id),
            // email -> ClaimTypes.Email (AuthController.Me ga cita)
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64)
        };
        // role -> ClaimTypes.Role, pa rade User.IsInRole i [Authorize(Roles)]
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: o.Issuer,
            audience: o.Audience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}