namespace MiniShop.Application.Services;

public sealed class CategoryService(
    ICategoryRepository repo,
    IUnitOfWork uow,
    IValidator<CategoryCreateDto> createValidator,
    IValidator<CategoryUpdateDto> updateValidator) : ICategoryService
{
    public Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct)
        => repo.GetAllAsync(ct);

    public async Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);
        return category is null ? NotFound<CategoryDto>(id) : Result.Success(category.ToDto());
    }

    public async Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken ct)
    {
        var v = await createValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) return Invalid<CategoryDto>(v);

        if (await repo.SlugExistsAsync(dto.Slug, null, ct))
            return DuplicateSlug<CategoryDto>(dto.Slug);

        var category = dto.ToEntity();
        repo.Add(category);
        await uow.SaveChangesAsync(ct);
        return Result.Success(category.ToDto());
    }

    public async Task<Result<CategoryDto>> UpdateAsync(
        int id, CategoryUpdateDto dto, CancellationToken ct)
    {
        var v = await updateValidator.ValidateAsync(dto, ct);
        if (!v.IsValid) return Invalid<CategoryDto>(v);

        var category = await repo.GetByIdAsync(id, ct);
        if (category is null) return NotFound<CategoryDto>(id);

        if (await repo.SlugExistsAsync(dto.Slug, id, ct))
            return DuplicateSlug<CategoryDto>(dto.Slug);

        category.Name = dto.Name.Trim();
        category.Slug = dto.Slug;
        await uow.SaveChangesAsync(ct);
        return Result.Success(category.ToDto());
    }

    public async Task<Result<bool>> DeleteAsync(int id, CancellationToken ct)
    {
        var category = await repo.GetByIdAsync(id, ct);
        if (category is null) return NotFound<bool>(id);

        // strani kljuc je Restrict: bez ove provere klijent bi dobio 500 iz baze
        if (await repo.HasProductsAsync(id, ct))
            return Result.Conflict<bool>("category-not-empty",
                "Kategorija ima proizvode i ne moze se obrisati.");

        repo.Remove(category);
        await uow.SaveChangesAsync(ct);
        return Result.Success(true);
    }

    private static Result<T> NotFound<T>(int id)
        => Result.NotFound<T>("category-not-found", $"Kategorija {id} ne postoji.");

    private static Result<T> DuplicateSlug<T>(string slug)
        => Result.Conflict<T>("duplicate-slug", $"Kategorija sa slugom '{slug}' vec postoji.");

    private static Result<T> Invalid<T>(FluentValidation.Results.ValidationResult v)
        => Result.Fail<T>(ErrorType.Validation, "validation",
            string.Join(" ", v.Errors.Select(e => e.ErrorMessage)));
}