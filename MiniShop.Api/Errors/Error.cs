namespace MiniShop.Api.Errors;

// Najava Result<T> iz nedelje 6: tada ErrorType i Error prelaze u
// Application/Common/Result.cs, a servis vraca Result<T> umesto null/bool.
public enum ErrorType { NotFound, Validation, Conflict, Forbidden, Failure }

// Code: stabilan, masinski (ide u type URL, ne prevodi se)
// Message: za coveka (ide u title, moze se prevesti)
public readonly record struct Error(ErrorType Type, string Code, string Message);
