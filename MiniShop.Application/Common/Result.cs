namespace MiniShop.Application.Common;

public enum ErrorType { NotFound, Validation, Conflict, Forbidden, Failure }

// Type -> HTTP status, Code -> type URL u ProblemDetails, Message -> title
public readonly record struct Error(ErrorType Type, string Code, string Message);

public sealed class Result<T>
{
    public T? Value { get; init; }
    public Error? Error { get; init; }
    public bool IsSuccess => Error is null;

    // railway: nastavi samo ako je uspeh
    public Result<TOut> Map<TOut>(Func<T, TOut> next) => IsSuccess
        ? new() { Value = next(Value!) }
        : new() { Error = Error };
}

public static class Result
{
    public static Result<T> Success<T>(T value) => new() { Value = value };

    public static Result<T> NotFound<T>(string code, string msg)
        => Fail<T>(ErrorType.NotFound, code, msg);

    public static Result<T> Conflict<T>(string code, string msg)
        => Fail<T>(ErrorType.Conflict, code, msg);

    public static Result<T> Forbidden<T>(string code, string msg)
        => Fail<T>(ErrorType.Forbidden, code, msg);

    public static Result<T> Fail<T>(ErrorType t, string code, string msg)
        => new() { Error = new(t, code, msg) };
}