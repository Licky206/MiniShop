namespace MiniShop.Api.Clients;

/// <summary>Tipizovani klijent servisa kursa. HttpClient stize iz IHttpClientFactory.</summary>
public sealed class ExchangeRateClient(HttpClient http)
{
    public async Task<decimal> GetEurRateAsync(CancellationToken ct)
    {
        using var resp = await http.GetAsync("/latest?base=RSD", ct);
        resp.EnsureSuccessStatusCode();   // 4xx i 5xx -> HttpRequestException

        var dto = await resp.Content.ReadFromJsonAsync<RatesDto>(ct)
            ?? throw new InvalidOperationException("Servis kursa je vratio prazan odgovor.");

        return dto.Rates.TryGetValue("EUR", out var eur)
            ? eur
            : throw new InvalidOperationException("U odgovoru servisa kursa nema EUR.");
    }
}

/// <summary>Odgovor: { "base": "RSD", "rates": { "EUR": 0.00853, "USD": 0.00925 } }.</summary>
public record RatesDto(Dictionary<string, decimal> Rates);