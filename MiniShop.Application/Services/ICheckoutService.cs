namespace MiniShop.Application.Services;

public interface ICheckoutService
{
    Task<Result<OrderDto>> PlaceOrderAsync(string userId, OrderCreateDto dto, CancellationToken ct);
}