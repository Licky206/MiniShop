namespace MiniShop.Application.Common;

// Data sloj prevodi DbUpdateConcurrencyException u ovaj izuzetak,
// pa Application ne zavisi od EF Core-a
public sealed class ConcurrencyException(string message, Exception? inner = null)
    : Exception(message, inner)
{
}