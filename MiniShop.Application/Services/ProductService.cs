namespace MiniShop.Application.Services;

public sealed class ProductService(
    IProductRepository repo,
    IUnitOfWork uow,
    IValidator<ProductCreateDto> createValidator,
    IValidator<ProductUpdateDto> updateValidator) : IProductService
{
    // citanje nema pravila: repozitorijum vraca gotovu projekciju
    public Task<PagedResult<ProductDto>> GetPageAsync(ProductQuery q, CancellationToken ct)
        => repo.GetPageAsync(q, ct);

    public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);
        return product is null
            ? NotFound<ProductDto>(id)
            : Result.Success(product.ToDto());
    }

    // verzija reda za ETag (nedelja 5); RowVersion je na pracenom entitetu iz repozitorijuma
    public async Task<byte[]?> GetRowVersionAsync(int id, CancellationToken ct)
        => (await repo.GetByIdAsync(id, ct))?.RowVersion;

    public async Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken ct)
    {
        var v = await createValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) return Invalid<ProductDto>(v);

        if (await repo.SkuExistsAsync(dto.Sku, ct))
            return Result.Conflict<ProductDto>("duplicate-sku", "Proizvod sa tim SKU vec postoji.");

        var product = dto.ToEntity();
        repo.Add(product);
        await uow.SaveChangesAsync(ct);            // INSERT; product.Id je sada popunjen

        // ponovo citamo da bi ToDto imao ucitanu kategoriju
        var saved = await repo.GetByIdAsync(product.Id, ct);
        return Result.Success(saved!.ToDto());
    }

    public async Task<Result<ProductDto>> UpdateAsync(
        int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var v = await updateValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) return Invalid<ProductDto>(v);

        var product = await repo.GetByIdAsync(id, ct);
        if (product is null) return NotFound<ProductDto>(id);

        if (dto.RowVersion is not null)
            repo.SetOriginalRowVersion(product, dto.RowVersion);

        product.ApplyUpdate(dto);
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (ConcurrencyException)
        {
            return Result.Conflict<ProductDto>("concurrency-conflict",
                "Proizvod je u medjuvremenu izmenjen. Ucitajte ga ponovo.");
        }

        // kategorija je mozda promenjena: citamo ponovo sa Include
        var saved = await repo.GetByIdAsync(id, ct);
        return Result.Success(saved!.ToDto());
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await repo.GetByIdAsync(id, ct);
        if (product is null) return NotFound<bool>(id);

        repo.Remove(product);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }

    private static Result<T> NotFound<T>(int id)
        => Result.NotFound<T>("product-not-found", $"Proizvod {id} ne postoji.");

    // sve poruke validatora u jednu recenicu (title u ProblemDetails)
    private static Result<T> Invalid<T>(FluentValidation.Results.ValidationResult v)
        => Result.Fail<T>(ErrorType.Validation, "validation",
            string.Join(" ", v.Errors.Select(e => e.ErrorMessage)));
}
