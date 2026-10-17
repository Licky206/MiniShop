namespace MiniShop.Api.Auth;

/// <summary>
/// Resource-based provera za Order: uspeva za administratora ili za vlasnika
/// narudzbenice. U ostalim slucajevima ne odlucuje, pa politika SameOwner pada.
/// </summary>
public sealed class OrderOwnerHandler : AuthorizationHandler<SameOwnerRequirement, Order>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, SameOwnerRequirement requirement, Order order)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // sub iz tokena (posle inbound mapiranja: ClaimTypes.NameIdentifier)
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null && order.UserId == userId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}