namespace MiniShop.Data;

public static class DataLayerExtensions
{
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services, IConfiguration config, bool isDevelopment = false)
    {
        services.AddDbContext<AppDbContext>(o =>
        {
            o.UseSqlServer(
                config.GetConnectionString("Default"),
                sql => sql.EnableRetryOnFailure(3));

            if (isDevelopment)
            {
                o.EnableSensitiveDataLogging();   // vrednosti parametara u logu - samo lokalno
                o.EnableDetailedErrors();
            }
        });

        // isti AppDbContext (jedan po zahtevu) je i unit of work
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IProductRepository, EfProductRepository>();
        services.AddScoped<ICategoryRepository, EfCategoryRepository>();
        services.AddScoped<IOrderRepository, EfOrderRepository>();
        return services;
    }

    // poziva se iz Program.cs posle Build(); scope jer je AppDbContext Scoped
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}