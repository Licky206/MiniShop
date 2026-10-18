namespace MiniShop.Api.Workers;

/// <summary>
/// Potrosac reda: za svaki posao otvara DI scope, uzima IOrderProcessor i obradjuje.
/// Izuzetak jednog posla ne sme da zaustavi servis.
/// </summary>
public sealed class OrderWorker(
    IOrderQueue queue,
    IServiceScopeFactory scopes,
    ILogger<OrderWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        log.LogInformation("OrderWorker pokrenut");
        try
        {
            await foreach (var job in queue.DequeueAllAsync(stoppingToken))
            {
                using var scope = scopes.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOrderProcessor>();
                try
                {
                    await processor.ProcessAsync(job, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    throw;   // gasenje aplikacije: izlazimo iz petlje
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Obrada narudzbenice {OrderId} nije uspela", job.OrderId);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // uredno gasenje hosta, nije greska
        }
        log.LogInformation("OrderWorker zaustavljen");
    }
}