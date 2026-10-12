namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    // podaci su u memoriji do nedelje 4 (baza); u nedelji 2 sele u servis
    private static readonly List<Product> _items =
    [
        new(1, "Tastatura", 4990m, 1),
        new(2, "Mis", 2490m, 1),
        new(3, "Monitor 27", 32990m, 2)
    ];

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetAll()
        => Ok(_items);

    [HttpGet("{id:int}")]
    public ActionResult<Product> GetById(int id)
    {
        var item = _items.FirstOrDefault(p => p.Id == id);

        return item is null
            ? NotFound()
            : Ok(item);
    }
}
