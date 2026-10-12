namespace MiniShop.Api.Models;

// privremeni model za nedelje 1-3; u nedelji 4 postaje EF entitet
public record Product(int Id, string Name, decimal Price, int CategoryId);
