namespace MiniShop.Api.Options;

public sealed class CatalogOptions
{
    public const string Section = "Catalog";

    [Range(1, 100)] public int PageSize { get; init; } = 20;
    [Range(0, 3600)] public int CacheSeconds { get; init; } = 60;
}
