namespace MiniShop.Api.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";          // Identity Id je string (nedelja 9)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Total { get; set; }                // racuna server iz UnitPrice
    public List<OrderItem> Items { get; set; } = [];
    public ShippingAddress? Shipping { get; set; }    // 1:1
}