namespace MiniShop.Api.Dtos;

// ulaz za PUT /api/products/{id}: ista polja kao kreiranje, bez Sku (ne menja se)
// i sa IsActive. PUT je potpuna zamena, pa klijent salje sva polja.
public sealed class ProductUpdateDto
{
    [Required(ErrorMessage = "Product_Name_Required")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Product_Name_Length")]
    public required string Name { get; init; }

    [StringLength(2000, ErrorMessage = "Product_Description_Length")]
    public string? Description { get; init; }

    [Range(0.01, 1_000_000, ErrorMessage = "Product_Price_Range")]
    public decimal Price { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Product_Stock_Range")]
    public int StockQuantity { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Product_Category_Range")]
    public int CategoryId { get; init; }

    // podrazumevano true: izostavljeno polje ne sme da sakrije proizvod
    public bool IsActive { get; init; } = true;
}
