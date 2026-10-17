public sealed class ProductService(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<ProductCreateDto> createValidator,
    IValidator<ProductUpdateDto> updateValidator,
    IExchangeRateProvider rates,        // nedelja 8
    IFileStorage storage) : IProductService   // nedelja 8
{
    // GetPageAsync, GetByIdAsync, CreateAsync, UpdateAsync i DeleteAsync
    // iz nedelja 6 i 7 ostaju ovde bez izmena; dodaju se samo dve metode ispod

    public async Task<Result<ProductDetailsDto>> GetDetailsAsync(int id, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);   // sa kategorijom (Include)
        if (product is null)
            return Result.NotFound<ProductDetailsDto>("product-not-found", "Proizvod ne postoji.");

        // kurs je dodatak: kada ga nema, PriceEur je null, a odgovor je i dalje uspeh
        var rate = await rates.GetEurRateAsync(ct);
        decimal? priceEur = rate is decimal r ? Math.Round(product.Price * r, 2) : null;

        return Result.Success(product.ToDetailsDto(priceEur));
    }

    public async Task<Result<string>> SetImageAsync(int id, string fileName, Stream content,
        string contentType, CancellationToken ct)
    {
        // prvo proizvod, pa fajl: bez proizvoda nema smisla cuvati sliku
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null)
            return Result.NotFound<string>("product-not-found", "Proizvod ne postoji.");

        var url = await storage.SaveAsync(fileName, content, contentType, ct);
        product.ImageUrl = url;
        product.UpdatedAt = DateTime.UtcNow;
        await uow.SaveChangesAsync(ct);
        return Result.Success(url);
    }
}