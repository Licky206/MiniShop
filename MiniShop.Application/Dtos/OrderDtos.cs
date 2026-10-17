namespace MiniShop.Application.Dtos;

public record OrderDto(
    int Id, DateTime CreatedAt, OrderStatus Status, decimal Total,
    IReadOnlyList<OrderItemDto> Items);

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

// korpa zivi na klijentu; API pri narucivanju prima stavke i adresu
public record OrderCreateDto(IReadOnlyList<OrderItemCreateDto> Items, ShippingAddressDto Shipping);

public record OrderItemCreateDto(int ProductId, int Quantity);

public record ShippingAddressDto(string Street, string City, string PostalCode, string Country);

// lista narudzbenica bez stavki (GET /orders/mine, nedelja 10)
public record OrderSummaryDto(int Id, DateTime CreatedAt, OrderStatus Status, decimal Total);
public record OrderStatusUpdateDto(OrderStatus Status);