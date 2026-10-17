namespace MiniShop.Application.Dtos;

// parametri iz query stringa: ?search=...&categoryId=1&sort=price_desc&page=2&pageSize=5
public record ProductQuery(
    string? Search, int? CategoryId, decimal? MinPrice, decimal? MaxPrice,
    string? Sort, int Page = 1, int PageSize = 20);