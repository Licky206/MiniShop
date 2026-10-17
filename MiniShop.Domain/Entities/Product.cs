namespace MiniShop.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sku { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // u kodu, ne u bazi
    public DateTime? UpdatedAt { get; set; }                     // ETag kataloga, nedelja 12
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Tag> Tags { get; set; } = [];                    // N:M (nedelja 5)

    // konkurentski token: SQL Server ga menja pri svakom UPDATE (nedelja 5);
    // od nedelje 6 isto kroz Fluent: IsRowVersion()
    [Timestamp]
    public byte[]? RowVersion { get; set; }
}
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public List<Product> Products { get; set; } = [];
}