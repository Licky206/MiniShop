namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ICheckoutService checkout, AppDbContext db) : ControllerBase
{
    // PRIVREMENO (nedelje 5-8): kupac iz zaglavlja X-Demo-User, bez provere identiteta.
    // Nedelja 9 uklanja zaglavlje; od nedelje 10 UserId dolazi iz JWT-a:
    //   User.FindFirstValue(ClaimTypes.NameIdentifier)
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "X-Demo-User"), Required, StringLength(450)] string userId,
        OrderCreateDto dto,
        CancellationToken ct)
    {
        var (order, error) = await checkout.PlaceOrderAsync(userId, dto, ct);
        if (error is { } e) return this.ToProblem(e);

        // 201 + Location: /api/orders/{id}
        return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
    }

    // za sada bez provere vlasnika: vlasnik ili Admin od nedelje 11
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        // samo citanje: AsNoTracking + projekcija, stavke i nazivi u jednom upitu
        var order = await db.Orders
            .AsNoTracking()
            .Where(o => o.Id == id)
            .Select(OrderMappings.Projection)
            .FirstOrDefaultAsync(ct);

        return order is null
            ? this.ToProblem(new Error(ErrorType.NotFound, "order-not-found",
                "Narudzbenica ne postoji."))
            : Ok(order);
    }
    //// VEZBA (privremeno, obrisati posle merenja): narudzbenice jednog kupca
    //[HttpGet("by-user/{userId}")]
    //public async Task<IActionResult> ByUser(string userId, CancellationToken ct)
    //{
    //    // LOSE: 1 upit za narudzbenice + 1 upit za stavke svake narudzbenice
    //    var orders = await db.Orders.Where(o => o.UserId == userId).ToListAsync(ct);
    //    foreach (var o in orders)
    //        await db.Entry(o).Collection(x => x.Items).LoadAsync(ct);

    //    return Ok(orders.Select(o => new OrderSummaryDto(o.Id, o.CreatedAt, o.Status,
    //        o.Items.Sum(i => i.Quantity * i.UnitPrice))));
    //}
}