namespace MiniShop.Api.Hubs;

/// <summary>
/// Hub na /hubs/orders. Klijent se pretplacuje na grupu order-{id};
/// server salje dogadjaj "statusChanged" preko IHubContext (SignalROrderNotifier).
/// </summary>
// [Authorize] od nedelje 10, kada postoji JWT (token stize kroz accessTokenFactory)
public sealed class OrderHub : Hub
{
    public static string GroupName(int orderId) => $"order-{orderId}";

    public Task Subscribe(int orderId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(orderId));

    public Task Unsubscribe(int orderId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(orderId));
}