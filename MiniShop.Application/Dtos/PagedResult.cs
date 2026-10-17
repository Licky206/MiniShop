namespace MiniShop.Application.Dtos;

// jedna strana rezultata; u nedelji 7 dobija TotalPages, HasNext i HasPrevious
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);