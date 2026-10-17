namespace MiniShop.Api.Controllers;

/// <summary>Katalog proizvoda.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController(IProductService svc) : ControllerBase
{
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

        // isti metapodaci i u zaglavlju: za HEAD zahteve i klijente koji ne citaju telo
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
    public async Task<ActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await svc.GetDetailsAsync(id, ct);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error!.Value);

        // ETag sa verzijom reda ostaje (nedelje 5 i 6)
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
}