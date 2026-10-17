namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController(ICategoryRepository categories) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> Get(CancellationToken ct)
        => Ok(await categories.GetAllAsync(ct));
}