namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController(
    UserManager<AppUser> users,
    TokenService tokens,
    RefreshTokenService refreshTokens) : ControllerBase
{
    /// <summary>Registracija kupca; uloga Customer.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };
        var result = await users.CreateAsync(user, dto.Password);   // hesuje lozinku
        if (!result.Succeeded)
            return ValidationProblem(ToModelState(result));        // 400 + detalji

        await users.AddToRoleAsync(user, "Customer");
        return Created("/api/v1/auth/me", null);                   // 201
    }

    /// <summary>Prijava: access token (15 min) i refresh token (14 dana).</summary>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(dto.Email);
        if (user is null || await users.IsLockedOutAsync(user))
            return Unauthorized();

        if (!await users.CheckPasswordAsync(user, dto.Password))
        {
            await users.AccessFailedAsync(user);                   // broji promasaje (lockout)
            return Unauthorized();
        }

        await users.ResetAccessFailedCountAsync(user);
        return Ok(await IssuePairAsync(user, ct));
    }

    /// <summary>Obnova: iskorisceni refresh token se menja za nov par tokena.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]                                               // access token je istekao
    public async Task<IActionResult> Refresh(RefreshDto dto, CancellationToken ct)
    {
        var rotated = await refreshTokens.RotateAsync(dto.RefreshToken, ct);
        if (rotated is null)
            return Unauthorized();

        var user = await users.FindByIdAsync(rotated.UserId);
        if (user is null || await users.IsLockedOutAsync(user))
        {
            await refreshTokens.RevokeFamilyAsync(rotated.UserId, ct);
            return Unauthorized();
        }

        // uloge se citaju ponovo: oduzeta uloga nestaje najkasnije za 15 minuta
        var roles = await users.GetRolesAsync(user);
        var (accessToken, expiresAt) = tokens.Create(user, roles);
        return Ok(new AuthResponse(accessToken, expiresAt, rotated.RefreshToken));
    }

    /// <summary>Odjava: opoziva refresh token ove sesije. Uvek 204.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]                                               // i sa isteklim access tokenom
    public async Task<IActionResult> Logout(RefreshDto dto, CancellationToken ct)
    {
        await refreshTokens.RevokeAsync(dto.RefreshToken, ct);
        return NoContent();
    }

    /// <summary>Podaci o prijavljenom korisniku iz tokena i baze.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;  // sub iz tokena
        var user = await users.FindByIdAsync(id);
        if (user is null)
            return Unauthorized();

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        return Ok(new { user.Id, user.Email, user.FullName, roles });
    }

    private async Task<AuthResponse> IssuePairAsync(AppUser user, CancellationToken ct)
    {
        var roles = await users.GetRolesAsync(user);
        var (accessToken, expiresAt) = tokens.Create(user, roles);
        var refreshToken = await refreshTokens.IssueAsync(user.Id, ct);
        return new AuthResponse(accessToken, expiresAt, refreshToken);
    }

    private ModelStateDictionary ToModelState(IdentityResult result)
    {
        foreach (var e in result.Errors)
            ModelState.AddModelError(e.Code, e.Description);
        return ModelState;
    }
}