namespace MiniShop.Application.Abstractions;

/// <summary>
/// Kurs RSD -> EUR. Application ne zna za HttpClient; implementacija je u Api sloju.
/// Vraca null kada kurs nije poznat (servis ne radi, a nema ni kesirane vrednosti).
/// </summary>
public interface IExchangeRateProvider
{
    Task<decimal?> GetEurRateAsync(CancellationToken ct);
}