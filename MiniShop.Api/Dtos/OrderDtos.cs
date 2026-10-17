namespace MiniShop.Api.Dtos;

// ---------- izlaz ----------
public record OrderDto(int Id, DateTime CreatedAt, OrderStatus Status, decimal Total,
    IReadOnlyList<OrderItemDto> Items);

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

// lista narudzbenica bez stavki (GET mine, nedelja 10)
public record OrderSummaryDto(int Id, DateTime CreatedAt, OrderStatus Status, decimal Total);

// ---------- ulaz ----------
// korpa zivi na klijentu: API prima samo Id proizvoda i kolicinu, NIKAD cenu.
// Kod record-a atributi validacije idu na parametre konstruktora.
public record OrderCreateDto(
    [Required, MinLength(1), MaxLength(50)] IReadOnlyList<OrderItemCreateDto> Items,
    [Required] ShippingAddressDto Shipping);

public record OrderItemCreateDto(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(1, 100)] int Quantity);

public record ShippingAddressDto(
    [Required, StringLength(200)] string Street,
    [Required, StringLength(100)] string City,
    [Required, StringLength(20)] string PostalCode,
    [Required, StringLength(2, MinimumLength = 2)] string Country);