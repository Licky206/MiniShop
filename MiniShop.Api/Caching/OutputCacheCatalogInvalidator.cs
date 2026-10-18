using Microsoft.AspNetCore.OutputCaching;

namespace MiniShop.Api.Caching;

public sealed class OutputCacheCatalogInvalidator(IOutputCacheStore store)
    : ICatalogCacheInvalidator
{
    public async Task InvalidateAsync(CancellationToken ct)
        => await store.EvictByTagAsync(CacheKeys.CatalogTag, ct);
}