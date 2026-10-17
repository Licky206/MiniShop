namespace MiniShop.Api.Services;

public sealed class CheckoutService(AppDbContext db, ILogger<CheckoutService> log)
    : ICheckoutService
{
    // POST /api/orders -> 201 (nedelja 5); od nedelje 13: 202 + pozadinska obrada
    public async Task<(OrderDto? Order, Error? Error)> PlaceOrderAsync(
        string userId, OrderCreateDto dto, CancellationToken ct)
    {
        // iste stavke spojene u jednu (kompozitni kljuc OrderId + ProductId)
        var lines = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new OrderItemCreateDto(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        // jedan upit za sve proizvode; PRACENI entiteti, jer menjamo StockQuantity
        var ids = lines.Select(l => l.ProductId).ToList();
        var found = await db.Products
            .Where(p => ids.Contains(p.Id))
            .ToListAsync(ct);

        if (found.Count != ids.Count)
            return (null, new Error(ErrorType.NotFound, "product-not-found",
                "Neki od trazenih proizvoda ne postoji."));

        var order = new Order
        {
            UserId = userId,
            Shipping = new ShippingAddress
            {
                Street = dto.Shipping.Street,
                City = dto.Shipping.City,
                PostalCode = dto.Shipping.PostalCode,
                Country = dto.Shipping.Country.ToUpperInvariant()
            }
        };

        foreach (var line in lines)
        {
            var product = found.First(p => p.Id == line.ProductId);

            // ranije izmene StockQuantity se ne cuvaju: SaveChanges se ne poziva
            if (!product.IsActive || product.StockQuantity < line.Quantity)
                return (null, new Error(ErrorType.Conflict, "insufficient-stock",
                    $"Nema dovoljno zaliha za proizvod '{product.Name}'."));

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
        db.Orders.Add(order);                                 // ceo graf: stavke + adresa

        try
        {
            // jedna transakcija: INSERT Orders, OrderItems, ShippingAddresses
            // + UPDATE Products (StockQuantity) ... WHERE RowVersion = @procitana
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // drugi kupac ili admin je promenio isti proizvod izmedju citanja i upisa
            log.LogWarning("Sukob pri naplati za korisnika {UserId}", userId);
            return (null, new Error(ErrorType.Conflict, "concurrency-conflict",
                "Stanje proizvoda se promenilo u medjuvremenu. Pokusajte ponovo."));
        }

        log.LogInformation("Narudzbenica {OrderId} za {UserId}: {Total} RSD",
            order.Id, userId, order.Total);
        return (order.ToDto(), null);
    }
}