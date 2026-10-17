namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get(
        [FromQuery] ProductQuery q, CancellationToken ct)
        => Ok(await svc.GetAllAsync(q, ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var product = await svc.GetByIdAsync(id, ct);
        if (product is null)
            return this.ToProblem(new Error(ErrorType.NotFound, "product-not-found",
                "Proizvod ne postoji."));

        // verzija koju klijent vraca u PUT-u kao rowVersion (bez navodnika)
        var version = await svc.GetRowVersionAsync(id, ct);
        if (version is not null)
            Response.Headers.ETag = $"\"{Convert.ToBase64String(version)}\"";

        return Ok(product);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Featured(CancellationToken ct)
        => Ok(await svc.GetAllAsync(
            new ProductQuery(null, null, null, null, "price_desc", 1, 5), ct));

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto, CancellationToken ct)
    {
        var created = await svc.CreateAsync(dto, ct);
        if (created is null)
            return this.ToProblem(new Error(ErrorType.Conflict, "duplicate-sku",
                "Proizvod sa tim SKU vec postoji."));

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var (_, error) = await svc.UpdateAsync(id, dto, ct);
        return error is { } e ? this.ToProblem(e) : NoContent();   // 404 ili 409
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => await svc.DeleteAsync(id, ct)
            ? NoContent()
            : this.ToProblem(new Error(ErrorType.NotFound, "product-not-found",
                "Proizvod ne postoji."));
}