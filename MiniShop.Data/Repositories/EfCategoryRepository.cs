namespace MiniShop.Data.Repositories;

public sealed class EfCategoryRepository(AppDbContext db) : ICategoryRepository
{
    public Task<bool> ExistsAsync(int id, CancellationToken ct)
        => db.Categories.AnyAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
        => await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(CategoryMappings.Projection)
            .ToListAsync(ct);

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct)
        => db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    // pri izmeni se sopstveni slug ne racuna kao zauzet
    public Task<bool> SlugExistsAsync(string slug, int? exceptId, CancellationToken ct)
        => db.Categories.AnyAsync(
            c => c.Slug == slug && (exceptId == null || c.Id != exceptId), ct);

    // i neaktivni proizvodi drze strani kljuc
    public Task<bool> HasProductsAsync(int id, CancellationToken ct)
        => db.Products.AnyAsync(p => p.CategoryId == id, ct);

    public void Add(Category category) => db.Categories.Add(category);

    public void Remove(Category category) => db.Categories.Remove(category);
}