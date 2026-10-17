namespace MiniShop.Domain.Entities;

public class ShippingAddress
{
    public int OrderId { get; set; }                  // PK i FK ka Orders (1:1)
    public Order Order { get; set; } = null!;
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";         // ISO 3166 alfa-2, npr. "RS"
}