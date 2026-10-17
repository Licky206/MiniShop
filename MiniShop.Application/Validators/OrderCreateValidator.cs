namespace MiniShop.Application.Validators;

public sealed class OrderCreateValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Narudzbenica mora imati bar jednu stavku.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .GreaterThan(0).WithMessage("Neispravan proizvod u stavci.");
            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, 100).WithMessage("Kolicina mora biti od 1 do 100.");
        });

        RuleFor(x => x.Shipping)
            .NotNull().WithMessage("Adresa za isporuku je obavezna.");

        // pravila adrese se izvrsavaju samo kad adresa postoji
        RuleFor(x => x.Shipping).ChildRules(address =>
        {
            address.RuleFor(a => a.Street).NotEmpty().MaximumLength(200);
            address.RuleFor(a => a.City).NotEmpty().MaximumLength(100);
            address.RuleFor(a => a.PostalCode).NotEmpty().MaximumLength(20);
            address.RuleFor(a => a.Country).NotEmpty().Length(2);   // ISO alfa-2, kolona nvarchar(2)
        });
    }
}