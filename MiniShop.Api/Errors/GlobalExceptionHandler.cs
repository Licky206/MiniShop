namespace MiniShop.Api.Errors;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> log,
    IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        if (ex is FluentValidation.ValidationException vex)
        {
            // greska klijenta (npr. ?sort=weight): 400 sa poljima, bez stack trace-a
            log.LogWarning("Neispravan zahtev na {Path}: {Message}", ctx.Request.Path, vex.Message);

            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var vpd = new ValidationProblemDetails(errors)
            {
                Type = "https://minishop.rs/errors/validation",
                Title = "Neispravan zahtev",
                Status = StatusCodes.Status400BadRequest,
                Instance = ctx.Request.Path
            };
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await ctx.Response.WriteAsJsonAsync(vpd, options: null,
                contentType: "application/problem+json", cancellationToken: ct);
            return true;
        }

        log.LogError(ex, "Neobradjena greska na {Path}", ctx.Request.Path);

        var pd = new ProblemDetails
        {
            Type = "https://minishop.rs/errors/internal",
            Title = "Greska na serveru",
            Status = StatusCodes.Status500InternalServerError,
            Instance = ctx.Request.Path,
            Detail = env.IsDevelopment() ? ex.ToString() : null   // detalji samo u razvoju
        };
        pd.Extensions["traceId"] = Activity.Current?.Id ?? ctx.TraceIdentifier;

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await ctx.Response.WriteAsJsonAsync(pd, options: null,
            contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}