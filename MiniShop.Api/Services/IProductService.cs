namespace MiniShop.Api.Services;

// U nedelji 6 povratni tipovi postaju Result<T>.
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductQuery q, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    // nedelja 5: verzija reda za ETag; null: proizvod ne postoji
    Task<byte[]?> GetRowVersionAsync(int id, CancellationToken ct);
    // null: proizvod sa tim SKU vec postoji
    Task<ProductDto?> CreateAsync(ProductCreateDto dto, CancellationToken ct);
    // nedelja 5: Error je product-not-found (404) ili concurrency-conflict (409)
    Task<(ProductDto? Product, Error? Error)> UpdateAsync(
        int id, ProductUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}