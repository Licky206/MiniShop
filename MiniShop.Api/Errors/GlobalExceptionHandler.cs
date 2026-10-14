namespace MiniShop.Api.Errors;

public sealed class GlobalExceptionHandler(
	ILogger<GlobalExceptionHandler> log) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext ctx, Exception ex, CancellationToken ct)
	{
		log.LogError(ex, "Neobradjena greska na {Path}", ctx.Request.Path);

		var pd = new ProblemDetails
		{
			Type = "https://minishop.rs/errors/internal",
			Title = "Greska na serveru",
			Status = StatusCodes.Status500InternalServerError,
			Instance = ctx.Request.Path
		};
		ctx.Response.StatusCode = pd.Status.Value;
		// bez contentType bi zaglavlje bilo application/json
		await ctx.Response.WriteAsJsonAsync(pd, options: null,
			contentType: "application/problem+json", cancellationToken: ct);
		return true;                  // obradjeno, ne ide dalje
	}
}
