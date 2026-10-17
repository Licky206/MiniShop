namespace MiniShop.Application.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken ct);
    Task<Result<CategoryDto>> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, CancellationToken ct);
}