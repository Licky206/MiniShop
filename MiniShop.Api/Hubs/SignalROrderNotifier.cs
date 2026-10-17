namespace MiniShop.Api.Hubs;

/// <summary>IOrderNotifier nad SignalR-om. IHubContext je Singleton, pa i ova klasa.</summary>
public sealed class SignalROrderNotifier(IHubContext<OrderHub> hub) : IOrderNotifier
{
    public Task StatusChangedAsync(int orderId, OrderStatus status, CancellationToken ct) =>
        hub.Clients.Group(OrderHub.GroupName(orderId))
            // status kao tekst: SignalR ima svoja JSON podesavanja, ne ona iz AddJsonOptions
            .SendAsync("statusChanged", new { id = orderId, status = status.ToString() }, ct);
}