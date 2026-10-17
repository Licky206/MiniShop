namespace MiniShop.Application.Validators;

public sealed class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv kategorije je obavezan.")
            .MaximumLength(80);
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug je obavezan.")
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug sme da sadrzi samo mala slova, cifre i crtice.");
    }
}

public sealed class CategoryUpdateValidator : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Naziv kategorije je obavezan.")
            .MaximumLength(80);
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug je obavezan.")
            .MaximumLength(80)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug sme da sadrzi samo mala slova, cifre i crtice.");
    }
}