namespace MiniShop.Api.Dtos;

// GET /api/products?search=mis&categoryId=1&minPrice=1000&sort=price&page=1&pageSize=20
// Kod record-a atributi validacije idu na PARAMETRE konstruktora (bez "property:"),
// inace MVC pri prvom zahtevu baca InvalidOperationException.
public record ProductQuery(
    [StringLength(100)] string? Search,
    [Range(1, int.MaxValue)] int? CategoryId,
    [Range(0.0, 1_000_000.0)] decimal? MinPrice,     // double granice: decimal se poredi
    [Range(0.0, 1_000_000.0)] decimal? MaxPrice,     // bez odsecanja na int
    [RegularExpression("^(name|name_desc|price|price_desc)$")] string? Sort,
    [Range(1, int.MaxValue)] int Page = 1,
    [Range(1, 100)] int PageSize = 20) : IValidatableObject
{
    // sopstveno pravilo nad dva polja: poziva se tek kad prodju atributi
    public IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (MinPrice is not null && MaxPrice is not null && MinPrice > MaxPrice)
            yield return new ValidationResult(
                "MinPrice ne sme biti veci od MaxPrice.", [nameof(MaxPrice)]);
    }
}
