namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ICheckoutService checkout, IOrderRepository orders) : ControllerBase
{
    // PRIVREMENO do nedelje 9: korisnik iz zaglavlja X-Demo-User, ne iz tokena
    [HttpPost]
    public async Task<IActionResult> Create(
        OrderCreateDto dto,
        [FromHeader(Name = "X-Demo-User")] string demoUser,
        CancellationToken ct)
    {
        var r = await checkout.PlaceOrderAsync(demoUser, dto, ct);
        return r.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = r.Value!.Id }, r.Value)
            : this.ToProblem(r.Error!.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null
            ? this.ToProblem(new Error(ErrorType.NotFound, "order-not-found",
                "Narudzbenica ne postoji."))
            : Ok(order.ToDto());
    }
}