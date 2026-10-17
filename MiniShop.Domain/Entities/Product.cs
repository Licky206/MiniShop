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
    public DateTime? UpdatedAt { get; set; }                     // ETag, nedelja 12
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public List<Tag> Tags { get; set; } = [];
    // optimisticka konkurentnost; IsRowVersion() je u ProductConfiguration
    public byte[]? RowVersion { get; set; }
}