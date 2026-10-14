namespace MiniShop.Api.Dtos;

// ulaz za POST /api/products: klijent sme da posalje tacno ova polja.
// ErrorMessage je KLJUC poruke; tekst je u Resources/Dtos.ProductCreateDto.*.resx
public sealed class ProductCreateDto
{
    [Required(ErrorMessage = "Product_Name_Required")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Product_Name_Length")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Product_Sku_Required")]
    [StringLength(32, ErrorMessage = "Product_Sku_Length")]
    public required string Sku { get; init; }

    [StringLength(2000, ErrorMessage = "Product_Description_Length")]
    public string? Description { get; init; }

    [Range(0.01, 1_000_000, ErrorMessage = "Product_Price_Range")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Product_Stock_Range")]
    public int StockQuantity { get; init; }

    // int je value tip: [Required] nema smisla (0 je "postavljeno"), zato Range
    [Range(1, int.MaxValue, ErrorMessage = "Product_Category_Range")]
    public int CategoryId { get; init; }

    // opciono polje, primer atributa [EmailAddress]; ne cuva se u katalogu
    [EmailAddress(ErrorMessage = "Product_Email_Invalid")]
    public string? SupplierEmail { get; init; }
}
