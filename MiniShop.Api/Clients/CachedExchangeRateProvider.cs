namespace MiniShop.Api.Clients;

/// <summary>
/// Kurs iz servisa, sa kesom od jednog minuta i fallback-om na poslednji poznat kurs.
/// Scoped: tipizovani klijent (Transient) ne sme da bude zarobljen u Singleton-u.
/// </summary>
public sealed class CachedExchangeRateProvider(
    ExchangeRateClient client,
    IMemoryCache cache,
    ILogger<CachedExchangeRateProvider> log) : IExchangeRateProvider
{
    private const string FreshKey = "rates:eur:fresh";   // kratko: ne zovemo servis na svaki zahtev
    private const string LastKey = "rates:eur:last";     // dugo: poslednji poznat kurs
    private static readonly TimeSpan FreshFor = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan KeepLastFor = TimeSpan.FromDays(1);

    public async Task<decimal?> GetEurRateAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(FreshKey, out decimal fresh))
            return fresh;

        try
        {
            var rate = await client.GetEurRateAsync(ct);
            cache.Set(FreshKey, rate, FreshFor);
            cache.Set(LastKey, rate, KeepLastFor);
            return rate;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            // HttpRequestException (5xx posle retry-ja, 401), TimeoutRejectedException,
            // BrokenCircuitException (otvoren prekidac), JsonException, InvalidOperationException
            if (cache.TryGetValue(LastKey, out decimal last))
            {
                log.LogWarning(ex,
                    "Servis kursa nije dostupan; koristi se poslednji poznat kurs {Rate}", last);
                return last;
            }

            log.LogWarning(ex, "Servis kursa nije dostupan, a kurs nije u kesu; PriceEur je null");
            return null;
        }
    }
}