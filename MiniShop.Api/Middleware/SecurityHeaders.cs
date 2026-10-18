namespace MiniShop.Api.Middleware;

/// <summary>Bezbednosna zaglavlja za browser klijente; postavljaju se pre odgovora.</summary>
public sealed class SecurityHeaders(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext ctx)
    {
        var h = ctx.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"] = "DENY";
        h["Referrer-Policy"] = "no-referrer";

        // Scalar UI (samo u razvoju) ucitava skripte i stilove, stroga CSP bi ga ugasila
        if (!ctx.Request.Path.StartsWithSegments("/scalar"))
            h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        return next(ctx);
    }
}