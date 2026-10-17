namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]                                     // svaka akcija trazi validan token
public class OrdersController(
    ICheckoutService checkout,
    IOrderRepository orders,
    IUnitOfWork uow,
    IOrderNotifier notifier,
    IAuthorizationService authz) : ControllerBase
{
    /// <summary>Kreira narudzbenicu prijavljenog korisnika.</summary>
    /// <remarks>Autorizacija: prijavljen korisnik.</remarks>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(OrderCreateDto dto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await checkout.PlaceOrderAsync(userId, dto, ct);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error!.Value);

        return CreatedAtAction(nameof(GetById),
            new { id = result.Value!.Id, version = "1" }, result.Value);
    }

    /// <summary>Narudzbenice prijavljenog korisnika, najnovije prve.</summary>
    /// <remarks>Autorizacija: prijavljen korisnik; vidi samo svoje.</remarks>
    [HttpGet("mine")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> Mine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await orders.GetMineAsync(userId, ct));
    }

    /// <summary>Narudzbenica sa stavkama.</summary>
    /// <remarks>Autorizacija: vlasnik ili Admin (politika SameOwner).</remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        // resurs se prvo ucita, pa se proverava pravo nad njim
        var order = await orders.GetByIdAsync(id, ct);
        if (order is null)
            return this.ToProblem(new Error(ErrorType.NotFound, "order-not-found",
                "Narudzbenica ne postoji."));

        var ok = await authz.AuthorizeAsync(User, order, "SameOwner");
        if (!ok.Succeeded)
            return this.ToProblem(new Error(ErrorType.Forbidden, "not-owner",
                "Narudzbenica pripada drugom korisniku."));

        return Ok(order.ToDto());
    }

    /// <summary>Promena statusa; klijenti dobijaju SignalR dogadjaj (nedelja 8).</summary>
    /// <remarks>Autorizacija: politika AdminOnly.</remarks>
    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "AdminOnly")]           // uz [Authorize] klase: prijava I uloga Admin
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetStatus(
        int id, OrderStatusUpdateDto dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(dto.Status))   // nedelja 8: {"status": 42} se odbija
            return this.ToProblem(new Error(ErrorType.Validation, "invalid-status",
                "Nepoznat status narudzbenice."));

        var order = await orders.GetByIdAsync(id, ct);
        if (order is null)
            return this.ToProblem(new Error(ErrorType.NotFound, "order-not-found",
                "Narudzbenica ne postoji."));

        order.Status = dto.Status;
        await uow.SaveChangesAsync(ct);
        await notifier.StatusChangedAsync(order.Id, order.Status, ct);
        return NoContent();
    }
}