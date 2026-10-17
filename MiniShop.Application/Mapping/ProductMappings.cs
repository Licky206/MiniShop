namespace MiniShop.Application.Mapping;

public static class ProductMappings
{
    // ide u IQueryable.Select - EF ga prevodi u SQL sa JOIN-om na Categories
    public static readonly Expression<Func<Product, ProductDto>> Projection = p =>
        new ProductDto(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity, p.Category!.Name);

    // nad ucitanim entitetom; bez ucitane kategorije ime je prazan string
    public static ProductDto ToDto(this Product p) =>
        new(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity, p.Category?.Name ?? "");

    public static ProductDetailsDto ToDetailsDto(this Product p, decimal? priceEur) =>
        new(p.Id, p.Name, p.Sku, p.Price, p.StockQuantity, p.Category?.Name ?? "",
            p.ImageUrl, priceEur);

    public static Product ToEntity(this ProductCreateDto d) => new()
    {
        Name = d.Name,
        Sku = d.Sku,
        Description = d.Description,
        Price = d.Price,
        StockQuantity = d.StockQuantity,
        CategoryId = d.CategoryId
    };

    // Sku se ne menja izmenom; UpdatedAt belezi trenutak izmene
    public static void ApplyUpdate(this Product p, ProductUpdateDto d)
    {
        p.Name = d.Name;
        p.Description = d.Description;
        p.Price = d.Price;
        p.StockQuantity = d.StockQuantity;
        p.CategoryId = d.CategoryId;
        p.IsActive = d.IsActive;
        p.UpdatedAt = DateTime.UtcNow;
    }


}