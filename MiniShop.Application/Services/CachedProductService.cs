namespace MiniShop.Application.Services;

/// <summary>
/// Dekorator nad IProductService: citanje kataloga kroz HybridCache (tag "catalog"),
/// invalidacija na svaku uspesnu izmenu. Poslovna pravila ostaju u ProductService.
/// </summary>
public sealed class CachedProductService(
    IProductService inner,
    HybridCache cache,
    ICatalogCacheInvalidator httpCache) : IProductService
{
    private static readonly string[] CatalogTags = [CacheKeys.CatalogTag];

    // L2 (Redis) 5 min, L1 (memorija instance) 1 min
    private static readonly HybridCacheEntryOptions PageOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(5),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    // pecat se cita na svaki GET kataloga, pa i on ide kroz kes
    private static readonly HybridCacheEntryOptions StampOptions = new()
    {
        Expiration = TimeSpan.FromSeconds(30),
        LocalCacheExpiration = TimeSpan.FromSeconds(30)
    };

    public async Task<PagedResult<ProductDto>> GetPageAsync(
        ProductQuery q, CancellationToken ct)
        => await cache.GetOrCreateAsync(
            CacheKeys.Catalog(q),
            async token => await inner.GetPageAsync(q, token),   // samo pri promasaju
            PageOptions,
            CatalogTags,
            ct);

    public async Task<CatalogStamp> GetCatalogStampAsync(CancellationToken ct)
        => await cache.GetOrCreateAsync(
            CacheKeys.CatalogStamp,
            async token => await inner.GetCatalogStampAsync(token),
            StampOptions,
            CatalogTags,
            ct);

    // pojedinacni proizvod se ne kesira: cita se retko, a mora biti tacan
    public Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct)
        => inner.GetByIdAsync(id, ct);

    // verzija reda za ETag detalja mora biti tacna: ne kesira se
    public Task<byte[]?> GetRowVersionAsync(int id, CancellationToken ct)
        => inner.GetRowVersionAsync(id, ct);

    // detalji (nedelja 8) se ne kesiraju ovde: kurs vec kesira CachedExchangeRateProvider
    public Task<Result<ProductDetailsDto>> GetDetailsAsync(int id, CancellationToken ct)
        => inner.GetDetailsAsync(id, ct);

    // nova slika menja UpdatedAt, pa i pecat kataloga
    public async Task<Result<string>> SetImageAsync(int id, string fileName, Stream content,
        string contentType, CancellationToken ct)
    {
        var result = await inner.SetImageAsync(id, fileName, content, contentType, ct);
        if (result.IsSuccess) await InvalidateCatalogAsync();
        return result;
    }

    public async Task<Result<ProductDto>> CreateAsync(
        ProductCreateDto dto, CancellationToken ct)
    {
        var result = await inner.CreateAsync(dto, ct);
        if (result.IsSuccess) await InvalidateCatalogAsync();
        return result;
    }

    public async Task<Result<ProductDto>> UpdateAsync(
        int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var result = await inner.UpdateAsync(id, dto, ct);
        if (result.IsSuccess) await InvalidateCatalogAsync();
        return result;
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var result = await inner.DeleteAsync(id, ct);
        if (result.IsSuccess) await InvalidateCatalogAsync();
        return result;
    }

    // izmena je vec upisana u bazu: invalidacija ne sme da bude otkazana
    // zato sto je klijent zatvorio konekciju, pa namerno CancellationToken.None
    private async Task InvalidateCatalogAsync()
    {
        await cache.RemoveByTagAsync(CacheKeys.CatalogTag, CancellationToken.None);
        await httpCache.InvalidateAsync(CancellationToken.None);
    }
}