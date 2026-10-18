namespace MiniShop.Application.Abstractions;

/// <summary>
/// Red poslova za obradu narudzbenica. Application sloj ne zna da li je
/// ispod Channel u memoriji, Redis lista ili Service Bus.
/// </summary>
public interface IOrderQueue
{
    ValueTask EnqueueAsync(OrderJob job, CancellationToken ct);
    IAsyncEnumerable<OrderJob> DequeueAllAsync(CancellationToken ct);
}