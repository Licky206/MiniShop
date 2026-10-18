namespace MiniShop.Application.Orders;

/// <summary>Obrada jedne narudzbenice: Pending -> Processing -> Confirmed.</summary>
public interface IOrderProcessor
{
    Task ProcessAsync(OrderJob job, CancellationToken ct);
}