namespace MiniShop.Application.Services;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPageAsync(ProductQuery q, CancellationToken ct);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct);
    // nedelja 5: verzija reda za ETag; null: proizvod ne postoji
    Task<byte[]?> GetRowVersionAsync(int id, CancellationToken ct);
    Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken ct);
    Task<Result<ProductDto>> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
}