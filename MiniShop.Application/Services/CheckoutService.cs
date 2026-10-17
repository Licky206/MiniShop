namespace MiniShop.Application.Services;

public sealed class CheckoutService(
    IProductRepository products,
    IOrderRepository orders,
    IUnitOfWork uow,
    IValidator<OrderCreateDto> validator) : ICheckoutService
{
    public async Task<Result<OrderDto>> PlaceOrderAsync(
        string userId, OrderCreateDto dto, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result.Fail<OrderDto>(ErrorType.Validation, "validation",
                string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));

        // iste stavke spojene u jednu (kompozitni kljuc OrderId + ProductId)
        var lines = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderItemCreateDto(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        var ids = lines.Select(l => l.ProductId).ToList();
        var found = await products.GetByIdsAsync(ids, ct);   // praceni entiteti
        if (found.Count != ids.Count)
            return Result.NotFound<OrderDto>("product-not-found",
                "Neki od trazenih proizvoda ne postoji.");

        var order = new Order
        {
            UserId = userId,
            Shipping = new ShippingAddress
            {
                Street = dto.Shipping.Street,
                City = dto.Shipping.City,
                PostalCode = dto.Shipping.PostalCode,
                Country = dto.Shipping.Country.ToUpperInvariant()   // "rs" -> "RS" (nedelja 5)
            }
        };

        foreach (var line in lines)
        {
            var product = found.First(p => p.Id == line.ProductId);
            if (!product.IsActive || product.StockQuantity < line.Quantity)
                return Result.Conflict<OrderDto>("insufficient-stock",
                    $"Nema dovoljno zaliha za proizvod '{product.Name}'.");

            product.StockQuantity -= line.Quantity;           // rezervacija zaliha
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = line.Quantity,
                UnitPrice = product.Price                     // cena iz baze, ne iz zahteva
            });
        }

        order.Total = order.Items.Sum(i => i.Quantity * i.UnitPrice);
        orders.Add(order);
        await uow.SaveChangesAsync(ct);   // jedna transakcija: Order + stavke + adresa + zalihe

        return Result.Success(order.ToDto());
    }
}