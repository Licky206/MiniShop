using Microsoft.AspNetCore.Identity;

namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AuthController(UserManager<AppUser> users, ILogger<AuthController> log)
    : ControllerBase
{
    /// <summary>Registracija kupca; nalog dobija ulogu Customer.</summary>
    /// <response code="201">Nalog je kreiran; Location vodi na GET me.</response>
    /// <response code="400">E-adresa je zauzeta ili lozinka ne ispunjava pravila.</response>
    [HttpPost("register")]
    [AllowAnonymous]   // potrebno od nedelje 11 (FallbackPolicy)
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName };

        // proverava politiku lozinke i jedinstvenost e-adrese, pa hesuje lozinku
        var result = await users.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ValidationProblem(ToModelState(result));

        await users.AddToRoleAsync(user, "Customer");
        log.LogInformation("Registrovan korisnik {UserId}", user.Id);   // nikad lozinka
        return Created("/api/v1/auth/me", new { user.Id, user.Email });
    }

    /// <summary>Provera e-adrese i lozinke uz brojanje promasaja (lockout).</summary>
    /// <response code="200">Prijava uspesna; od nedelje 10 telo je AuthResponse.</response>
    /// <response code="401">Pogresni podaci ili zakljucan nalog (ista poruka).</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(dto.Email);

        // ista poruka za nepostojeci nalog, zakljucan nalog i pogresnu lozinku
        if (user is null || await users.IsLockedOutAsync(user))
            return Unauthorized();

        if (!await users.CheckPasswordAsync(user, dto.Password))
        {
            // CheckPasswordAsync ne dira brojac; na 5. promasaj nalog se zakljucava
            await users.AccessFailedAsync(user);
            log.LogWarning("Neuspela prijava za korisnika {UserId}", user.Id);
            return Unauthorized();
        }

        await users.ResetAccessFailedCountAsync(user);
        var roles = await users.GetRolesAsync(user);

        // nedelja 10: tokeni, pa Ok(new AuthResponse(token, exp, refresh))
        return Ok(new { userId = user.Id, email = user.Email, roles });
    }

    /// <summary>Podaci o prijavljenom korisniku iz claimsa (radi od nedelje 10).</summary>
    /// <response code="401">Nema tokena ili token ne vazi.</response>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        // Id samo iz claimsa, nikad iz zahteva
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null) return Unauthorized();

        var user = await users.FindByIdAsync(id);
        if (user is null) return Unauthorized();   // nalog obrisan posle izdavanja tokena

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        return Ok(new { user.Id, user.Email, user.FullName, roles });
    }

    // IdentityResult -> ModelState, da 400 ima isti oblik kao automatska validacija
    private ModelStateDictionary ToModelState(IdentityResult result)
    {
        foreach (var e in result.Errors)
            ModelState.AddModelError(e.Code, e.Description);
        return ModelState;
    }
}