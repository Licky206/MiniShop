namespace MiniShop.Api.Services;

// Isti interfejs u nedelji 4 implementira ProductService nad AppDbContext-om.
// U nedelji 6 povratni tipovi postaju Result<T>.
public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductQuery q, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    // null: proizvod sa tim SKU vec postoji
    Task<ProductDto?> CreateAsync(ProductCreateDto dto, CancellationToken ct);
    // null: proizvod ne postoji
    Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
