namespace MiniShop.Api.Controllers;

/// <summary>Kategorije proizvoda. GET je javan; izmene od nedelje 11 samo Admin.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CategoriesController(ICategoryService svc) : ControllerBase
{
    /// <summary>Sve kategorije, po nazivu.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Get(CancellationToken ct)
        => Ok(await svc.GetAllAsync(ct));

    /// <summary>Jedna kategorija.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var r = await svc.GetByIdAsync(id, ct);
        return r.IsSuccess ? Ok(r.Value) : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Kreira kategoriju.</summary>
    /// <response code="201">Kreirana; Location vodi na novu kategoriju.</response>
    /// <response code="409">Slug je zauzet.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CategoryCreateDto dto, CancellationToken ct)
    {
        var r = await svc.CreateAsync(dto, ct);
        return r.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = r.Value!.Id, version = "1" }, r.Value)
            : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Menja naziv i slug kategorije.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto, CancellationToken ct)
    {
        var r = await svc.UpdateAsync(id, dto, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Brise praznu kategoriju.</summary>
    /// <response code="409">Kategorija ima proizvode.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var r = await svc.DeleteAsync(id, ct);
        return r.IsSuccess ? NoContent() : this.ToProblem(r.Error!.Value);
    }
}