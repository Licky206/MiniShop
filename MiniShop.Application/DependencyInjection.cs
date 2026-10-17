namespace MiniShop.Application;

public static class ApplicationLayerExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICheckoutService, CheckoutService>();

        // svi AbstractValidator<T> iz ovog asemblija kao IValidator<T>;
        // Scoped, jer validatori koriste repozitorijume
        services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>(
            ServiceLifetime.Scoped);
        return services;
    }
}