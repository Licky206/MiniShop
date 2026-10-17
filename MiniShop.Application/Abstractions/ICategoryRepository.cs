namespace MiniShop.Application.Abstractions;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);

    // nedelja 7: CRUD kategorija
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, int? exceptId, CancellationToken ct);
    Task<bool> HasProductsAsync(int id, CancellationToken ct);
    void Add(Category category);
    void Remove(Category category);
}