namespace MiniShop.Application;

public static class ApplicationLayerExtensions
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICheckoutService, CheckoutService>();

        // svi AbstractValidator<T> iz ovog asemblija kao IValidator<T> (Scoped)
        services.AddValidatorsFromAssemblyContaining<ProductCreateValidator>(
            ServiceLifetime.Scoped);
        return services;
    }
}