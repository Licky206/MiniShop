namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IProductService svc) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Get(
        [FromQuery] ProductQuery q, CancellationToken ct)
    {
        var page = await svc.GetPageAsync(q, ct);
        return Ok(page.Items);   // oblik odgovora kao u fazi 5; PagedResult od nedelje 7
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var r = await svc.GetByIdAsync(id, ct);
        if (!r.IsSuccess) return this.ToProblem(r.Error!.Value);

        // verzija reda u ETag-u, kao u petoj nedelji: klijent je vraca u PUT-u kao rowVersion
        var version = await svc.GetRowVersionAsync(id, ct);
        if (version is not null)
            Response.Headers.ETag = $"\"{Convert.ToBase64String(version)}\"";

        return Ok(r.Value);
    }

    [HttpGet("featured")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> Featured(CancellationToken ct)
    {
        var page = await svc.GetPageAsync(
            new ProductQuery(null, null, null, null, "price_desc", 1, 5), ct);
        return Ok(page.Items);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto, CancellationToken ct)
    {
        var r = await svc.CreateAsync(dto, ct);
        return r.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, r.Value)
            : this.ToProblem(r.Error!.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto, CancellationToken ct)
    {
        var r = await svc.UpdateAsync(id, dto, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await svc.DeleteAsync(id, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }
}