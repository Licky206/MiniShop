namespace MiniShop.Api.Filters;

// Action filter: zna akciju i vezane argumente (middleware to ne zna).
// Registruje se globalno u Program.cs; beleze se samo izmene (POST, PUT, DELETE).
public sealed class AuditFilter(ILogger<AuditFilter> log) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var method = ctx.HttpContext.Request.Method;
        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            await next();                  // citanje se ne audituje
            return;
        }

        var action = ctx.ActionDescriptor.DisplayName;
        var user = ctx.HttpContext.User.Identity?.Name ?? "anonimno";   // nedelja 9
        log.LogInformation("Audit -> {Method} {Action} ({User}), argumenata: {Count}",
            method, action, user, ctx.ActionArguments.Count);

        var executed = await next();       // izvrsi akciju

        if (executed.Exception is not null && !executed.ExceptionHandled)
        {
            log.LogWarning("Audit <- {Action}: izuzetak {Exception}",
                action, executed.Exception.GetType().Name);
            return;
        }

        // rezultat jos nije upisan u odgovor: status citamo iz samog rezultata
        int? status = executed.Result switch
        {
            ObjectResult o => o.StatusCode ?? StatusCodes.Status200OK,
            StatusCodeResult s => s.StatusCode,
            _ => null
        };
        log.LogInformation("Audit <- {Action}: {Status}", action, status);
    }
}
