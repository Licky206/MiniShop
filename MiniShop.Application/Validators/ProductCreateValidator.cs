namespace MiniShop.Application.Validators;

public sealed class ProductCreateValidator : AbstractValidator<ProductCreateDto>
{
    public ProductCreateValidator(ICategoryRepository categories)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(120);

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU je obavezan.")
            .MaximumLength(32);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Cena mora biti pozitivna.")
            .LessThan(1_000_000);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Zaliha ne moze biti negativna.");

        // nepostojeca kategorija u telu zahteva je greska ulaza -> 400
        RuleFor(x => x.CategoryId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Kategorija je obavezna.")
            .MustAsync(async (id, ct) => await categories.ExistsAsync(id, ct))
            .WithMessage("Kategorija ne postoji.");
    }
}