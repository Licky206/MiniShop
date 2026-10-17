namespace MiniShop.Api.Dtos;

public static class OrderMappings
{
    // za IQueryable.Select: stavke i nazivi proizvoda u jednom SQL-u, bez Include
    public static readonly Expression<Func<Order, OrderDto>> Projection = o =>
        new OrderDto(o.Id, o.CreatedAt, o.Status, o.Total,
            o.Items
                .OrderBy(i => i.ProductId)
                .Select(i => new OrderItemDto(
                    i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice))
                .ToList());

    // lista bez stavki: Total je vec u koloni, nema potrebe za SUM
    public static readonly Expression<Func<Order, OrderSummaryDto>> SummaryProjection = o =>
        new OrderSummaryDto(o.Id, o.CreatedAt, o.Status, o.Total);

    // nad ucitanim agregatom: Items i Items[].Product moraju biti u memoriji
    public static OrderDto ToDto(this Order o) =>
        new(o.Id, o.CreatedAt, o.Status, o.Total,
            o.Items
                .OrderBy(i => i.ProductId)
                .Select(i => new OrderItemDto(
                    i.ProductId, i.Product.Name, i.Quantity, i.UnitPrice))
                .ToList());
}