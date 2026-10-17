namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OrdersController(
    ICheckoutService checkout,
    IOrderRepository orders,
    IUnitOfWork uow,
    IOrderNotifier notifier) : ControllerBase
{
    /// <summary>Kreira narudzbenicu za prijavljenog korisnika.</summary>
    /// <response code="201">Narudzbenica je kreirana.</response>
    /// <response code="401">Korisnik nije prijavljen.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        OrderCreateDto dto,
        CancellationToken ct)
    {
        // Id korisnika samo iz claimsa; X-Demo-User vise ne postoji.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
            return Challenge();

        var result = await checkout.PlaceOrderAsync(userId, dto, ct);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error!.Value);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = result.Value!.Id,
                version = "1"
            },
            result.Value);
    }

    /// <summary>Narudzbenica sa stavkama i nazivima proizvoda.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);

        return order is null
            ? this.ToProblem(new Error(
                ErrorType.NotFound,
                "order-not-found",
                "Narudzbenica ne postoji."))
            : Ok(order.ToDto());
    }

    /// <summary>Menja status i javlja ga SignalR klijentima grupe order-{id}.</summary>
    /// <response code="204">Status je promenjen.</response>
    [HttpPost("{id:int}/status")]
    // privremeno bez autorizacije; od nedelje 11: [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetStatus(
        int id,
        OrderStatusUpdateDto dto,
        CancellationToken ct)
    {
        // JsonStringEnumConverter prima i broj, pa {"status": 42} mora da se odbije ovde
        if (!Enum.IsDefined(dto.Status))
            return this.ToProblem(new Error(
                ErrorType.Validation,
                "invalid-status",
                "Nepoznat status narudzbenice."));

        var order = await orders.GetByIdAsync(id, ct);

        if (order is null)
            return this.ToProblem(new Error(
                ErrorType.NotFound,
                "order-not-found",
                "Narudzbenica ne postoji."));

        order.Status = dto.Status;

        await uow.SaveChangesAsync(ct);                                // prvo baza
        await notifier.StatusChangedAsync(order.Id, order.Status, ct); // pa obavestenje

        return NoContent();
    }
}