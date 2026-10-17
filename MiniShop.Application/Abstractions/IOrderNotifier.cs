namespace MiniShop.Application.Abstractions;

/// <summary>
/// Javlja klijentima promenu statusa narudzbenice. Application ne zna za SignalR;
/// koriste ga OrdersController (nedelja 8) i OrderProcessor (nedelja 13).
/// </summary>
public interface IOrderNotifier
{
    Task StatusChangedAsync(int orderId, OrderStatus status, CancellationToken ct);
}