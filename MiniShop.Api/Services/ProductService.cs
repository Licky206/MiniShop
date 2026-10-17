namespace MiniShop.Api.Services;

public sealed class ProductService(AppDbContext db, ILogger<ProductService> log)
    : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(
        ProductQuery q, CancellationToken ct)
    {
        // citanje: AsNoTracking (projekcija ionako ne prati, ali namera je jasna)
        IQueryable<Product> query = db.Products.AsNoTracking().Where(p => p.IsActive);

        var search = q.Search;
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));
        if (q.CategoryId is int categoryId)
            query = query.Where(p => p.CategoryId == categoryId);
        if (q.MinPrice is decimal min)
            query = query.Where(p => p.Price >= min);
        if (q.MaxPrice is decimal max)
            query = query.Where(p => p.Price <= max);

        query = q.Sort switch
        {
            "price" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "name_desc" => query.OrderByDescending(p => p.Name),
            _ => query.OrderBy(p => p.Name)
        };

        // paginacija + projekcija: jedan SQL, samo kolone iz ProductDto
        return await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(ProductMappings.Projection)
            .ToListAsync(ct);
    }

    public Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct) =>
        db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ProductMappings.Projection)
            .FirstOrDefaultAsync(ct);

    public Task<byte[]?> GetRowVersionAsync(int id, CancellationToken ct) =>
        db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => p.RowVersion)
            .FirstOrDefaultAsync(ct);

    public async Task<ProductDto?> CreateAsync(ProductCreateDto dto, CancellationToken ct)
    {
        // unique indeks bi to odbio i sam, ali sa DbUpdateException umesto jasnog odgovora
        if (await db.Products.AnyAsync(p => p.Sku == dto.Sku, ct))
        {
            log.LogWarning("Pokusaj kreiranja proizvoda sa zauzetim SKU {Sku}", dto.Sku);
            return null;
        }

        var product = dto.ToEntity();
        db.Products.Add(product);                 // Added
        await db.SaveChangesAsync(ct);            // INSERT, Id i RowVersion su popunjeni
        log.LogInformation("Kreiran proizvod {Id} ({Sku})", product.Id, product.Sku);

        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<(ProductDto? Product, Error? Error)> UpdateAsync(
        int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return (null, new Error(ErrorType.NotFound, "product-not-found",
                "Proizvod ne postoji."));

        product.ApplyUpdate(dto);                 // Modified

        // klijent salje verziju koju je video; bez ovoga EF poredi sa svezom
        // verzijom iz ovog zahteva i sukob se nikad ne otkrije
        if (dto.RowVersion is not null)
            db.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            // UPDATE Products SET ... WHERE Id = @id AND RowVersion = @original
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            log.LogWarning("Sukob izmena za proizvod {Id}", id);
            return (null, new Error(ErrorType.Conflict, "concurrency-conflict",
                "Proizvod je izmenjen u medjuvremenu. Osvezite podatke i pokusajte ponovo."));
        }

        return (await GetByIdAsync(id, ct), null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is null) return false;

        db.Products.Remove(product);              // Deleted
        await db.SaveChangesAsync(ct);            // DELETE (Restrict ako je u narudzbenici)
        return true;
    }
}