namespace MiniShop.Data.Repositories;

public sealed class EfProductRepository(AppDbContext db) : IProductRepository
{
    private const int MaxPageSize = 100;

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
        // granice: pageSize 1..100 (DoS), page tako da (page - 1) * size ne prekoraci int
        var size = Math.Clamp(q.PageSize, 1, MaxPageSize);
        var page = Math.Clamp(q.Page, 1, int.MaxValue / MaxPageSize);

        // 1) filter: samo zadati parametri, svaka kombinacija dobija jednostavan SQL
        IQueryable<Product> query = db.Products.AsNoTracking().Where(p => p.IsActive);

        var search = q.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));
        if (q.CategoryId is int categoryId)
            query = query.Where(p => p.CategoryId == categoryId);
        if (q.MinPrice is decimal min)
            query = query.Where(p => p.Price >= min);
        if (q.MaxPrice is decimal max)
            query = query.Where(p => p.Price <= max);

        // 2) sort iz whitelist-a + ThenBy(Id); nepoznat kljuc -> 400 pre ikakvog upita
        var sorted = ProductSortMap.Apply(query, q.Sort);

        // 3) COUNT nad filtriranim upitom, pre Skip/Take
        var total = await query.CountAsync(ct);

        // 4) strana i 5) projekcija: jedan SELECT sa OFFSET/FETCH i samo kolonama DTO-a
        var items = await sorted
            .Skip((page - 1) * size)
            .Take(size)
            .Select(ProductMappings.Projection)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, page, size, total);
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct)
        => db.Products.AnyAsync(p => p.Sku == sku, ct);

    public void SetOriginalRowVersion(Product product, byte[] rowVersion)
        => db.Entry(product).Property(p => p.RowVersion).OriginalValue = rowVersion;

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);
}