namespace MiniShop.Domain.Entities;

// 1:1 sa Order; OrderId je istovremeno primarni i strani kljuc
public class ShippingAddress
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
}