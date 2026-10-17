namespace MiniShop.Data.Repositories;

public sealed class EfProductRepository(AppDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(
        IReadOnlyCollection<int> ids, CancellationToken ct)
        => await db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

    public async Task<PagedResult<ProductDto>> GetPageAsync(ProductQuery q, CancellationToken ct)
    {
        IQueryable<Product> query = db.Products.Where(p => p.IsActive);

        var search = q.Search;
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));
        if (q.CategoryId is int categoryId)
            query = query.Where(p => p.CategoryId == categoryId);
        if (q.MinPrice is decimal min)
            query = query.Where(p => p.Price >= min);
        if (q.MaxPrice is decimal max)
            query = query.Where(p => p.Price <= max);

        var total = await query.CountAsync(ct);

        // privremeno kao u nedelji 4; whitelist mapa i ThenBy(Id) stizu u nedelji 7
        query = q.Sort switch
        {
            "price" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_desc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(ProductMappings.Projection)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, q.Page, q.PageSize, total);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct)
        => db.Products.AnyAsync(p => p.Sku == sku, ct);

    public void SetOriginalRowVersion(Product product, byte[] rowVersion)
        => db.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);
}