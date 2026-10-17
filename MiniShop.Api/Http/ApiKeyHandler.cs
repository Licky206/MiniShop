namespace MiniShop.Api.Http;

/// <summary>
/// Izlazni middleware: API kljuc i korelacioni Id na svaki zahtev, plus log sa trajanjem.
/// Stoji ispod resilience pipeline-a, pa se izvrsava jednom po pokusaju.
/// </summary>
public sealed class ApiKeyHandler(IOptions<RatesOptions> opt, ILogger<ApiKeyHandler> log)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        // retry salje isti HttpRequestMessage vise puta: Remove pa Add, da nema duplikata
        request.Headers.Remove("X-Api-Key");
        if (!string.IsNullOrEmpty(opt.Value.ApiKey))
            request.Headers.Add("X-Api-Key", opt.Value.ApiKey);

        request.Headers.Remove("X-Correlation-Id");
        request.Headers.Add("X-Correlation-Id", Activity.Current?.Id ?? "-");

        var sw = Stopwatch.StartNew();
        var response = await base.SendAsync(request, ct);   // dalje kroz lanac
        log.LogInformation("{Method} {Uri} -> {Status} za {Ms} ms",
            request.Method, request.RequestUri, (int)response.StatusCode, sw.ElapsedMilliseconds);
        return response;
    }
}