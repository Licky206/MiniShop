namespace MiniShop.Application.Dtos;

/// <summary>Detalji proizvoda: polja ProductDto + ImageUrl + PriceEur (null bez kursa).</summary>
public record ProductDetailsDto(
    int Id,
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    string Category,
    string? ImageUrl,
    decimal? PriceEur);