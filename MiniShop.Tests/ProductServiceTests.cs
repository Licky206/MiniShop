namespace MiniShop.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    public ProductServiceTests()
    {
        // podrazumevano: kategorija 1 postoji (MustAsync u validatorima)
        _categories
            .Setup(c => c.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    // pravi validatori nad laznim repozitorijumom kategorija
    private ProductService CreateSut() => new(
        _repo.Object,
        _uow.Object,
        new ProductCreateValidator(_categories.Object),
        new ProductUpdateValidator(_categories.Object));

    [Fact]
    public async Task CreateAsync_sku_zauzet_vraca_Conflict()
    {
        _repo.Setup(r => r.SkuExistsAsync("KB-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().CreateAsync(
            new ProductCreateDto("Tastatura", "KB-100", null, 4990m, 10, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Type.Should().Be(ErrorType.Conflict);
        result.Error!.Value.Code.Should().Be("duplicate-sku");
        _repo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ispravan_proizvod_cuva_i_vraca_dto()
    {
        _repo.Setup(r => r.SkuExistsAsync("KB-100", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        // baza dodeljuje Id pri SaveChanges; ovde ga dodeljujemo u Add
        _repo.Setup(r => r.Add(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 5);
        // ponovno citanje posle SaveChanges vraca proizvod sa kategorijom
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product
            {
                Id = 5,
                Name = "Tastatura",
                Sku = "KB-100",
                Price = 4990m,
                StockQuantity = 10,
                CategoryId = 1,
                Category = new Category { Id = 1, Name = "Periferija", Slug = "periferija" }
            });

        var result = await CreateSut().CreateAsync(
            new ProductCreateDto("Tastatura", "KB-100", null, 4990m, 10, 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(new ProductDto(5, "Tastatura", "KB-100", 4990m, 10, "Periferija"));
        _repo.Verify(r => r.Add(It.Is<Product>(p => p.Sku == "KB-100")), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProductCreateValidator_prijavljuje_naziv_cenu_i_kategoriju()
    {
        _categories
            .Setup(c => c.ExistsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        var validator = new ProductCreateValidator(_categories.Object);

        var result = await validator.TestValidateAsync(
            new ProductCreateDto("", "KB-100", null, -5m, 10, 99));

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Naziv je obavezan.");
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Cena mora biti pozitivna.");
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Kategorija ne postoji.");
        result.ShouldNotHaveValidationErrorFor(x => x.Sku);
    }
}