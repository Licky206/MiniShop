namespace MiniShop.Api.Controllers;

/// <summary>Narudzbenice.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController(ICheckoutService checkout, IOrderRepository orders) : ControllerBase
{
    /// <summary>Prima stavke korpe i adresu i kreira narudzbenicu.</summary>
    /// <remarks>PRIVREMENO do nedelje 9: Id korisnika u zaglavlju X-Demo-User.</remarks>
    /// <response code="201">Kreirana; Location vodi na narudzbenicu.</response>
    /// <response code="409">Nema dovoljno zaliha.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        OrderCreateDto dto,
        [FromHeader(Name = "X-Demo-User")] string demoUser,
        CancellationToken ct)
    {
        var r = await checkout.PlaceOrderAsync(demoUser, dto, ct);
        return r.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = r.Value!.Id, version = "1" }, r.Value)
            : this.ToProblem(r.Error!.Value);
    }

    /// <summary>Narudzbenica sa stavkama.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);
        return order is null
            ? this.ToProblem(new Error(ErrorType.NotFound, "order-not-found",
                "Narudzbenica ne postoji."))
            : Ok(order.ToDto());
    }
}