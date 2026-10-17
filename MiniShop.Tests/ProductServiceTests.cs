namespace MiniShop.Tests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repo = new();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private readonly Mock<IExchangeRateProvider> _rates = new();
    private readonly Mock<IFileStorage> _files = new();

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
     new ProductUpdateValidator(_categories.Object),
     _rates.Object,
     _files.Object);

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
    [Fact]
    public async Task GetDetailsAsync_sa_kursom_racuna_cenu_u_evrima()
    {
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
        _rates.Setup(r => r.GetEurRateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)0.0085m);

        var result = await CreateSut().GetDetailsAsync(5, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        // 4990 * 0.0085 = 42.415 -> Math.Round(..., 2) (banker's rounding) = 42.42
        result.Value!.PriceEur.Should().Be(42.42m);
    }

    [Fact]
    public async Task GetDetailsAsync_bez_kursa_cena_u_evrima_je_null()
    {
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
        _rates.Setup(r => r.GetEurRateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((decimal?)null);

        var result = await CreateSut().GetDetailsAsync(5, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PriceEur.Should().BeNull();
    }

    [Fact]
    public async Task GetDetailsAsync_nepostojeci_proizvod_vraca_NotFound_bez_poziva_kursa()
    {
        _repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await CreateSut().GetDetailsAsync(7, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Type.Should().Be(ErrorType.NotFound);
        result.Error!.Value.Code.Should().Be("product-not-found");
        _rates.Verify(r => r.GetEurRateAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetImageAsync_cuva_fajl_i_postavlja_ImageUrl()
    {
        var product = new Product { Id = 5, Name = "Tastatura", Sku = "KB-100", Price = 4990m };
        _repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        using var stream = new MemoryStream([1, 2, 3]);
        _files.Setup(f => f.SaveAsync("kb.png", stream, "image/png", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/images/kb.png");

        var result = await CreateSut().SetImageAsync(5, "kb.png", stream, "image/png", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("/images/kb.png");
        product.ImageUrl.Should().Be("/images/kb.png");
        product.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetImageAsync_nepostojeci_proizvod_ne_dira_storage()
    {
        _repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await CreateSut().SetImageAsync(
            7, "kb.png", Stream.Null, "image/png", CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Type.Should().Be(ErrorType.NotFound);
        _files.VerifyNoOtherCalls();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

}