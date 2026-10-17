namespace MiniShop.Application.Abstractions;

public interface IOrderRepository
{
    // praceni entitet sa stavkama, proizvodima i adresom
    Task<Order?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Order order);
    Task<IReadOnlyList<OrderSummaryDto>> GetMineAsync(string userId, CancellationToken ct);
}