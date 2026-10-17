namespace MiniShop.Api.Models;

// tok: Pending -> Processing -> Confirmed -> Shipped; Cancelled dok nije poslata
// (pozadinska promena statusa: nedelja 13)
public enum OrderStatus { Pending, Processing, Confirmed, Shipped, Cancelled }