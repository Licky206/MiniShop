namespace MiniShop.Api.Workers;

/// <summary>Red poslova u memoriji nad ogranicenim Channel-om. Singleton.</summary>
public sealed class ChannelOrderQueue : IOrderQueue
{
    private readonly Channel<OrderJob> _channel = Channel.CreateBounded<OrderJob>(
        new BoundedChannelOptions(capacity: 500)
        {
            FullMode = BoundedChannelFullMode.Wait,   // proizvodjac ceka kad je red pun
            SingleReader = true,                      // jedan OrderWorker cita
            SingleWriter = false                      // vise zahteva pise istovremeno
        });

    public ValueTask EnqueueAsync(OrderJob job, CancellationToken ct)
        => _channel.Writer.WriteAsync(job, ct);

    public IAsyncEnumerable<OrderJob> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}