namespace MiniShop.Domain.Entities;

// spojna tabela sa podacima; kompozitni kljuc (OrderId, ProductId)
public class OrderItem
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }            // cena u trenutku kupovine
}