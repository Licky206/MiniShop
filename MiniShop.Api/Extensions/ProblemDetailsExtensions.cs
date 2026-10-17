namespace MiniShop.Api.Extensions;

public static class ProblemDetailsExtensions
{
    // Error iz servisa -> ProblemDetails: type iz Code (nikad se ne prevodi),
    // title preveden po kodu za kulturu zahteva, a ako prevoda nema - poruka iz servisa
    public static IActionResult ToProblem(this ControllerBase c, Error e)
    {
        var localizer = c.HttpContext.RequestServices
            .GetService<IStringLocalizer<ErrorMessages>>();
        var translated = localizer?[e.Code];
        var title = translated is { ResourceNotFound: false } ? translated.Value : e.Message;

        return c.Problem(
            type: $"https://minishop.rs/errors/{e.Code}",
            title: title,
            statusCode: e.Type.ToStatusCode());
    }

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}