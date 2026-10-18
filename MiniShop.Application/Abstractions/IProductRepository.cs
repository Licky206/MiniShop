namespace MiniShop.Application.Abstractions;

public interface IProductRepository
{
    // praceni entitet sa ucitanom kategorijom (za izmenu i brisanje)
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);

    // praceni entiteti za checkout: jedan upit za sve stavke korpe
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);

    // projekcija u DTO; puna verzija (clamp, whitelist sortiranja): nedelja 7
    Task<PagedResult<ProductDto>> GetPageAsync(ProductQuery q, CancellationToken ct);

    Task<bool> SkuExistsAsync(string sku, CancellationToken ct);

    // klijent salje verziju koju je video; EF je stavlja u WHERE uslov UPDATE-a
    void SetOriginalRowVersion(Product product, byte[] rowVersion);

    void Add(Product product);
    void Remove(Product product);
    /// <summary>MAX(COALESCE(UpdatedAt, CreatedAt)) i COUNT(*) nad Products.</summary>
    Task<CatalogStamp> GetCatalogStampAsync(CancellationToken ct);
}   