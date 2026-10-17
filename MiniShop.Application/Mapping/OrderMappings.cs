namespace MiniShop.Application.Mapping;

public static class OrderMappings
{
    // stavke moraju imati ucitan Product (Include u EfOrderRepository ili dodela u checkout-u)
    public static OrderDto ToDto(this Order o) => new(
        o.Id,
        o.CreatedAt,
        o.Status,
        o.Total,
        o.Items
            .Select(i => new OrderItemDto(
                i.ProductId, i.Product?.Name ?? "", i.Quantity, i.UnitPrice))
            .ToList());
}