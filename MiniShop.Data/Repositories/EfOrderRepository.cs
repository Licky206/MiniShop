namespace MiniShop.Data.Repositories;

public sealed class EfOrderRepository(AppDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(int id, CancellationToken ct)
        => db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Shipping)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public void Add(Order order) => db.Orders.Add(order);
}