namespace MiniShop.Application.Validators;

public sealed class ProductUpdateValidator : AbstractValidator<ProductUpdateDto>
{
    public ProductUpdateValidator(ICategoryRepository categories)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv je obavezan.")
            .MaximumLength(120);

        RuleFor(x => x.Description).MaximumLength(2000);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Cena mora biti pozitivna.")
            .LessThan(1_000_000);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Zaliha ne moze biti negativna.");

        RuleFor(x => x.CategoryId)
            .Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Kategorija je obavezna.")
            .MustAsync(async (id, ct) => await categories.ExistsAsync(id, ct))
            .WithMessage("Kategorija ne postoji.");
    }
}