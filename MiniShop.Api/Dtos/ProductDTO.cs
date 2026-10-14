namespace MiniShop.Api.Dtos;

// izlazni ugovor sa klijentom; kanonski oblik koji ostaje do poslednjeg predavanja
public record ProductDto(
    int Id,
    string Name,
    string Sku,
    decimal Price,
    int StockQuantity,
    string Category);
