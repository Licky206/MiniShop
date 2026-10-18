namespace MiniShop.Application.Abstractions;

/// <summary>
/// Brise kesirane HTTP odgovore kataloga. Application ne zna da je ispod
/// Output caching; implementacija je u Api sloju.
/// </summary>
public interface ICatalogCacheInvalidator
{
    Task InvalidateAsync(CancellationToken ct);
}