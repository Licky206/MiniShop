namespace MiniShop.Application.Dtos;

// Read: ono sto klijent dobija; ime kategorije je vec spljosteno
public record ProductDto(
    int Id, string Name, string Sku, decimal Price, int StockQuantity, string Category);

// Create: bez Id-ja (dodeljuje ga baza)
public record ProductCreateDto(
    string Name, string Sku, string? Description,
    decimal Price, int StockQuantity, int CategoryId);

// Update: Id dolazi iz rute, Sku se ne menja;
// RowVersion je verzija reda koju je klijent video (nedelja 5)
public record ProductUpdateDto(
    string Name, string? Description, decimal Price,
    int StockQuantity, int CategoryId, bool IsActive, byte[]? RowVersion = null);