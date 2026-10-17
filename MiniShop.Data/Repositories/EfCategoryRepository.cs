namespace MiniShop.Data.Repositories;

public sealed class EfCategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<bool> ExistsAsync(int id, CancellationToken ct)
        => db.Categories.AnyAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
        => await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Slug))
            .ToListAsync(ct);
}