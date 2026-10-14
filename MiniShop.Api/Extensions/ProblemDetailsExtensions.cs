namespace MiniShop.Api.Extensions;

public static class ProblemDetailsExtensions
{
    // jedno mesto na kom se vrsta greske prevodi u HTTP status i ProblemDetails
    public static IActionResult ToProblem(this ControllerBase c, Error e)
    {
        var status = e.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        // Problem() koristi ProblemDetailsFactory: dodaje traceId i
        // Content-Type application/problem+json
        return c.Problem(
            type: $"https://minishop.rs/errors/{e.Code}",
            title: e.Message,
            statusCode: status,
            instance: c.HttpContext.Request.Path);
    }
}
