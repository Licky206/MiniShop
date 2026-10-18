namespace MiniShop.Tests;

/// <summary>
/// Pokrece pravi MiniShop.Api u memoriji. Svaka instanca ima svoj SQLite fajl,
/// pa jedan test = jedna fabrika = jedna baza.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"minishop-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");   // bez MigrateAsync i bez HSTS-a za localhost

        // Jwt sekcija iz Tests/appsettings.Testing.json, iznad appsettings.json iz Api-ja
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddJsonFile(TestJwt.SettingsPath, optional: false));

        builder.ConfigureServices(services =>
        {
            // EF Core 9+: provider je registrovan kao IDbContextOptionsConfiguration<T>
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite($"Data Source={_dbPath}"));

            // bez Redisa u testovima: HybridCache dobija L2 u memoriji
            services.RemoveAll<IDistributedCache>();
            services.AddDistributedMemoryCache();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // sema iz modela (ne SQL Server migracije) + kategorije iz HasData
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();
        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        SqliteConnection.ClearAllPools();   // otpusti fajl pre brisanja
        File.Delete(_dbPath);
    }
}