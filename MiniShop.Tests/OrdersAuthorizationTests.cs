namespace MiniShop.Tests;

public class OrdersAuthorizationTests
{
    private static readonly Order AnasOrder = new() { Id = 41, UserId = "ana" };

    private static ClaimsPrincipal Principal(string? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId is not null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }

    private static async Task<AuthorizationHandlerContext> EvaluateAsync(
        ClaimsPrincipal user, Order order)
    {
        var context = new AuthorizationHandlerContext([new SameOwnerRequirement()], user, order);
        await new OrderOwnerHandler().HandleAsync(context);
        return context;
    }

    [Fact]
    public async Task Owner_is_authorized_for_own_order()
    {
        var context = await EvaluateAsync(Principal("ana", "Customer"), AnasOrder);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Other_customer_is_not_authorized_for_foreign_order()
    {
        var context = await EvaluateAsync(Principal("marko", "Customer"), AnasOrder);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Admin_is_authorized_for_any_order()
    {
        var context = await EvaluateAsync(Principal("admin-1", "Admin"), AnasOrder);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task User_without_id_claim_does_not_own_order_with_empty_owner()
    {
        var context = await EvaluateAsync(Principal(null), new Order { Id = 7, UserId = "" });

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task SameOwner_policy_rejects_foreign_order_through_authorization_service()
    {
        // ista registracija kao u Program.cs, bez web hosta
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorizationCore(o =>
            o.AddPolicy("SameOwner", p => p.Requirements.Add(new SameOwnerRequirement())));
        services.AddSingleton<IAuthorizationHandler, OrderOwnerHandler>();
        await using var provider = services.BuildServiceProvider();
        var authz = provider.GetRequiredService<IAuthorizationService>();

        var result = await authz.AuthorizeAsync(
            Principal("marko", "Customer"), AnasOrder, "SameOwner");

        result.Succeeded.Should().BeFalse();
    }
}