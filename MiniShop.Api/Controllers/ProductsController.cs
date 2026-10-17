namespace MiniShop.Api.Controllers;

/// <summary>Katalog proizvoda.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(IProductService svc) : ControllerBase
{
    // dozvoljeni tipovi: MIME tip -> (magicni bajtovi, ekstenzija na disku)
    private static readonly Dictionary<string, (byte[] Magic, string Ext)> ImageTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = (new byte[] { 0xFF, 0xD8, 0xFF }, ".jpg"),
            ["image/png"] = (new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png")
        };

    /// <summary>Vraca paginiranu listu aktivnih proizvoda.</summary>
    /// <remarks>
    /// Filteri: search (deo naziva), categoryId, minPrice, maxPrice.
    /// Sortiranje: name, price, created, category; sufiks _desc za opadajuci redosled.
    /// pageSize je ogranicen na 100. Metapodaci su i u zaglavlju X-Pagination.
    /// </remarks>
    /// <response code="200">Paginirana kolekcija proizvoda.</response>
    /// <response code="400">Nedozvoljeno polje za sortiranje.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get(
        [FromQuery] ProductQuery q, CancellationToken ct)
    {
        var page = await svc.GetPageAsync(q, ct);

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
        {
            page.Page,
            page.PageSize,
            page.TotalCount,
            page.TotalPages,
            page.HasNext,
            page.HasPrevious
        }, JsonSerializerOptions.Web);

        return Ok(page);
    }

    /// <summary>Detalji proizvoda, sa cenom u evrima kada je kurs poznat.</summary>
    /// <response code="200">Proizvod; polje priceEur izostaje kada kurs nije poznat.</response>
    /// <response code="404">Proizvod ne postoji.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await svc.GetDetailsAsync(id, ct);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error!.Value);

        var version = await svc.GetRowVersionAsync(id, ct);
        if (version is not null)
            Response.Headers.ETag = $"\"{Convert.ToBase64String(version)}\"";

        return Ok(result.Value);
    }

    /// <summary>Pet najskupljih aktivnih proizvoda.</summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Featured(CancellationToken ct)
    {
        var page = await svc.GetPageAsync(
            new ProductQuery(null, null, null, null, "price_desc", 1, 5), ct);
        return Ok(page.Items);
    }

    /// <summary>Kreira proizvod (od nedelje 11 samo Admin).</summary>
    /// <response code="201">Kreiran; Location vodi na novi proizvod.</response>
    /// <response code="400">Neispravan ulaz ili nepostojeca kategorija.</response>
    /// <response code="409">Proizvod sa tim SKU vec postoji.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(ProductCreateDto dto, CancellationToken ct)
    {
        var r = await svc.CreateAsync(dto, ct);
        return r.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = r.Value!.Id, version = "1" }, r.Value)
            : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Menja proizvod (od nedelje 11 samo Admin).</summary>
    /// <response code="204">Izmenjeno.</response>
    /// <response code="404">Proizvod ne postoji.</response>
    /// <response code="409">Proizvod je u medjuvremenu izmenjen (RowVersion).</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var r = await svc.UpdateAsync(id, dto, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Brise proizvod (od nedelje 11 samo Admin).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await svc.DeleteAsync(id, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Postavlja sliku proizvoda (JPEG ili PNG, najvise 5 MB).</summary>
    /// <response code="200">Slika je sacuvana; telo sadrzi njen URL.</response>
    /// <response code="400">Fajl nije poslat, prazan je ili sadrzaj ne odgovara tipu.</response>
    /// <response code="404">Proizvod ne postoji.</response>
    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB: vece telo se odbija pre citanja
    // privremeno bez autorizacije; od nedelje 11: [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UploadImage(
        int id, IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return FileProblem("Fajl je prazan.");

        if (!ImageTypes.TryGetValue(file.ContentType, out var type))
            return FileProblem("Dozvoljeni su samo JPEG i PNG.");

        if (file.Length < type.Magic.Length)
            return FileProblem("Fajl je prekratak da bi bio slika.");

        await using var stream = file.OpenReadStream();

        var head = new byte[type.Magic.Length];
        await stream.ReadExactlyAsync(head, ct);

        if (!head.SequenceEqual(type.Magic))
            return FileProblem("Sadrzaj fajla ne odgovara tipu.");

        stream.Position = 0;

        // Ime pravimo sami: GUID + ekstenzija iz whitelist-e,
        // nikad FileName od klijenta.
        var name = $"{Guid.NewGuid():N}{type.Ext}";

        var result = await svc.SetImageAsync(
            id, name, stream, file.ContentType, ct);

        return result.IsSuccess
            ? Ok(new { url = result.Value })
            : this.ToProblem(result.Error!.Value);
    }

    // 400 u istom obliku kao automatska validacija: errors.file = [poruka]
    private IActionResult FileProblem(string message)
    {
        ModelState.AddModelError("file", message);
        return ValidationProblem(ModelState);
    }
}