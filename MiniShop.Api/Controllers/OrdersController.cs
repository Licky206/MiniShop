namespace MiniShop.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class OrdersController(
    ICheckoutService checkout,
    IOrderRepository orders,
    IUnitOfWork uow,
    IOrderNotifier notifier,
    IAuthorizationService authz) : ControllerBase
{
    /// <summary>Prima stavke korpe i adresu; obrada se nastavlja u pozadini.</summary>
    /// <remarks>Autorizacija: prijavljen korisnik.</remarks>
    /// <response code="202">Narudzbenica primljena; Location vodi na njen status.</response>
    /// <response code="401">Korisnik nije prijavljen.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        OrderCreateDto dto,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var result = await checkout.PlaceOrderAsync(userId, dto, ct);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error!.Value);

        // 202 umesto 201:
        // resurs postoji, ali obrada jos traje (Status = Pending).
        return AcceptedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id, version = "1" },
            result.Value);
    }

    /// <summary>Narudzbenica sa stavkama; vlasnik ili Admin.</summary>
    /// <remarks>Autorizacija: vlasnik ili Admin (politika SameOwner).</remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken ct)
    {
        // Resurs se prvo ucita, pa se proverava pravo nad njim.
        var order = await orders.GetByIdAsync(id, ct);

        if (order is null)
            return this.ToProblem(
                new Error(
                    ErrorType.NotFound,
                    "order-not-found",
                    "Narudzbenica ne postoji."));

        var ok = await authz.AuthorizeAsync(
            User,
            order,
            "SameOwner");

        if (!ok.Succeeded)
            return this.ToProblem(
                new Error(
                    ErrorType.Forbidden,
                    "not-owner",
                    "Narudzbenica pripada drugom korisniku."));

        return Ok(order.ToDto());
    }

    /// <summary>Narudzbenice prijavljenog korisnika, najnovije prve.</summary>
    /// <remarks>Autorizacija: prijavljen korisnik; vidi samo svoje.</remarks>
    [HttpGet("mine")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> Mine(
        CancellationToken ct)
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        return Ok(await orders.GetMineAsync(userId, ct));
    }

    /// <summary>Kupac otkazuje svoju narudzbenicu dok nije poslata.</summary>
    /// <remarks>Autorizacija: vlasnik ili Admin (politika SameOwner).</remarks>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(
        int id,
        CancellationToken ct)
    {
        var order = await orders.GetByIdAsync(id, ct);

        if (order is null)
            return this.ToProblem(
                new Error(
                    ErrorType.NotFound,
                    "order-not-found",
                    "Narudzbenica ne postoji."));

        var ok = await authz.AuthorizeAsync(
            User,
            order,
            "SameOwner");

        if (!ok.Succeeded)
            return this.ToProblem(
                new Error(
                    ErrorType.Forbidden,
                    "not-owner",
                    "Narudzbenica pripada drugom korisniku."));

        if (order.Status is OrderStatus.Shipped or OrderStatus.Cancelled)
            return this.ToProblem(
                new Error(
                    ErrorType.Conflict,
                    "order-not-cancellable",
                    $"Narudzbenica u statusu {order.Status} se ne moze otkazati."));

        // Rezervisane zalihe se vracaju.
        // Items.Product je ucitan u EfOrderRepository.
        foreach (var item in order.Items)
            item.Product.StockQuantity += item.Quantity;

        order.Status = OrderStatus.Cancelled;

        await uow.SaveChangesAsync(ct);

        await notifier.StatusChangedAsync(
            order.Id,
            order.Status,
            ct);

        return NoContent();
    }

    /// <summary>Admin rucno menja status; klijenti dobijaju SignalR dogadjaj.</summary>
    /// <remarks>Autorizacija: politika AdminOnly.</remarks>
    [HttpPost("{id:int}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetStatus(
        int id,
        OrderStatusUpdateDto dto,
        CancellationToken ct)
    {
        if (!Enum.IsDefined(dto.Status))
            return this.ToProblem(
                new Error(
                    ErrorType.Validation,
                    "invalid-status",
                    "Nepoznat status narudzbenice."));

        var order = await orders.GetByIdAsync(id, ct);

        if (order is null)
            return this.ToProblem(
                new Error(
                    ErrorType.NotFound,
                    "order-not-found",
                    "Narudzbenica ne postoji."));

        order.Status = dto.Status;

        await uow.SaveChangesAsync(ct);

        await notifier.StatusChangedAsync(
            order.Id,
            order.Status,
            ct);

        return NoContent();
    }
}