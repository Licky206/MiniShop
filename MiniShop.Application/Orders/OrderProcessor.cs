namespace MiniShop.Application.Orders;

public sealed class OrderProcessor(
    IOrderRepository orders,
    IUnitOfWork uow,
    IOrderNotifier notifier,
    ILogger<OrderProcessor> log) : IOrderProcessor
{
    public async Task ProcessAsync(OrderJob job, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(job.OrderId, ct);
        if (order is null)
        {
            log.LogWarning("Narudzbenica {OrderId} ne postoji, posao se preskace", job.OrderId);
            return;
        }

        // idempotentnost: dupliran posao ili vec obradjena narudzbenica
        if (order.Status != OrderStatus.Pending)
        {
            log.LogInformation("Narudzbenica {OrderId} je vec u statusu {Status}",
                order.Id, order.Status);
            return;
        }

        await MoveAsync(order, OrderStatus.Processing, ct);

        // ovde ide stvarni posao: naplata, provera kod dobavljaca, e-posta...
        // kratko cekanje simulira trajanje da bi se promena statusa videla kroz SignalR
        await Task.Delay(TimeSpan.FromSeconds(2), ct);

        await MoveAsync(order, OrderStatus.Confirmed, ct);
    }

    private async Task MoveAsync(Order order, OrderStatus status, CancellationToken ct)
    {
        order.Status = status;
        await uow.SaveChangesAsync(ct);
        await notifier.StatusChangedAsync(order.Id, status, ct);   // SignalR (nedelja 8)
        log.LogInformation("Narudzbenica {OrderId}: {Status}", order.Id, status);
    }
}