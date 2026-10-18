namespace MiniShop.Tests;

/// <summary>
/// Pravi pipeline: autentifikacija, autorizacija, validacija, kes i ETag nad SQLite-om.
/// xUnit pravi novu instancu klase po testu, pa i novu fabriku i bazu.
/// </summary>
public sealed class ProductsApiTests : IDisposable
{
    private const string ProductsUrl = "/api/v1/products";
    private readonly ApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private HttpClient ClientAs(string? role)
    {
        var client = _factory.CreateClient();
        if (role is not null)
            client.DefaultRequestHeaders.Authorization =
                new("Bearer", TestJwt.For($"{role.ToLowerInvariant()}@minishop.rs", role));
        return client;
    }

    // kategorija 1 ("Periferija") postoji iz HasData
    private static ProductCreateDto Keyboard(string sku = "KB-100") =>
        new("Tastatura", sku, null, 4990m, 10, 1);

    [Fact]
    public async Task Admin_moze_da_kreira_proizvod()
    {
        var res = await ClientAs("Admin").PostAsJsonAsync(ProductsUrl, Keyboard());

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        res.Headers.Location.Should().NotBeNull();
        var body = await res.Content.ReadFromJsonAsync<ProductDto>();
        body!.Sku.Should().Be("KB-100");
        body.Category.Should().Be("Periferija");
    }

    [Fact]
    public async Task Kupac_ne_moze_da_kreira_proizvod()
    {
        var res = await ClientAs("Customer").PostAsJsonAsync(ProductsUrl, Keyboard());

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Anonimni_korisnik_ne_moze_da_kreira_proizvod()
    {
        var res = await ClientAs(null).PostAsJsonAsync(ProductsUrl, Keyboard());

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Katalog_vraca_304_za_aktuelni_ETag()
    {
        var client = ClientAs(null);
        var first = await client.GetAsync(ProductsUrl);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        first.Headers.ETag.Should().NotBeNull();

        using var request = new HttpRequestMessage(HttpMethod.Get, ProductsUrl);
        request.Headers.IfNoneMatch.Add(first.Headers.ETag!);
        var second = await client.SendAsync(request);

        second.StatusCode.Should().Be(HttpStatusCode.NotModified);
    }

    [Fact]
    public async Task Izmena_kataloga_menja_ETag()
    {
        var anonymous = ClientAs(null);
        var before = await anonymous.GetAsync(ProductsUrl);
        var oldTag = before.Headers.ETag!;

        (await ClientAs("Admin").PostAsJsonAsync(ProductsUrl, Keyboard("KB-200")))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        using var request = new HttpRequestMessage(HttpMethod.Get, ProductsUrl);
        request.Headers.IfNoneMatch.Add(oldTag);
        var after = await anonymous.SendAsync(request);

        // invalidacija iz nedelje 12: output kes i HybridCache su obrisani
        after.StatusCode.Should().Be(HttpStatusCode.OK);
        after.Headers.ETag!.Tag.Should().NotBe(oldTag.Tag);
    }

    [Fact]
    public async Task Odgovor_nosi_bezbednosna_zaglavlja()
    {
        var res = await ClientAs(null).GetAsync(ProductsUrl);

        res.Headers.GetValues("X-Content-Type-Options").Should().Equal("nosniff");
        res.Headers.GetValues("X-Frame-Options").Should().Equal("DENY");
        res.Headers.Contains("Content-Security-Policy").Should().BeTrue();
    }
}