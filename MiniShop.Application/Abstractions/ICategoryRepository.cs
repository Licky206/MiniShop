namespace MiniShop.Application.Abstractions;

public interface ICategoryRepository
{
    Task<bool> ExistsAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
}