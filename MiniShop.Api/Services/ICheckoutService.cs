namespace MiniShop.Api.Services;

// (Order, Error): tacno jedno je popunjeno - najava Result<OrderDto> iz nedelje 6
public interface ICheckoutService
{
    Task<(OrderDto? Order, Error? Error)> PlaceOrderAsync(
        string userId, OrderCreateDto dto, CancellationToken ct);
}