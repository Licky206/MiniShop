namespace MiniShop.Api.Extensions;

public static class ProblemDetailsExtensions
{
    // Error iz servisa -> ProblemDetails: type iz Code, title iz Message, status iz Type
    public static IActionResult ToProblem(this ControllerBase c, Error e) =>
        c.Problem(
            type: $"https://minishop.rs/errors/{e.Code}",
            title: e.Message,
            statusCode: e.Type.ToStatusCode());

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status500InternalServerError
    };
}