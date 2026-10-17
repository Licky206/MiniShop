namespace MiniShop.Data.Repositories;

// jedino mesto koje zna po cemu klijent sme da sortira katalog
public static class ProductSortMap
{
    private static readonly Dictionary<string, Expression<Func<Product, object>>> Allowed =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = p => p.Name,
            ["price"] = p => p.Price,
            ["created"] = p => p.CreatedAt,
            ["category"] = p => p.Category!.Name
        };

    public static IReadOnlyCollection<string> Keys => Allowed.Keys;

    // "price" ili "price_asc" -> rastuce, "price_desc" -> opadajuce; prazno -> "name"
    public static IQueryable<Product> Apply(IQueryable<Product> query, string? sort)
    {
        var value = string.IsNullOrWhiteSpace(sort) ? "name" : sort.Trim();
        var parts = value.Split('_');
        var desc = parts.Length == 2
            && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
        var asc = parts.Length == 1
            || (parts.Length == 2 && parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase));

        if (!(asc || desc) || !Allowed.TryGetValue(parts[0], out var expr))
        {
            // 400 preko GlobalExceptionHandler-a; puno ime zbog DataAnnotations.ValidationException
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("sort",
                    $"Sortiranje '{value}' nije podrzano. Dozvoljeno: " +
                    $"{string.Join(", ", Allowed.Keys)}, uz sufiks _desc za opadajuci redosled.")
            });
        }

        var ordered = desc ? query.OrderByDescending(expr) : query.OrderBy(expr);
        return ordered.ThenBy(p => p.Id);   // stabilan redosled izmedju strana
    }
}