namespace MiniShop.Data;

/// <summary>
/// Uloge Admin i Customer + admin nalog iz konfiguracije (Seed:AdminEmail,
/// Seed:AdminPassword iz User Secrets). Idempotentno: sme pri svakom startu.
/// </summary>
public static class IdentitySeeder
{
    public static readonly string[] Roles = ["Admin", "Customer"];

    public static async Task SeedAsync(IServiceProvider sp, IConfiguration cfg)
    {
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var r in Roles)
            if (!await roles.RoleExistsAsync(r))
                await roles.CreateAsync(new IdentityRole(r));

        var email = cfg["Seed:AdminEmail"];
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Seed:AdminEmail nije podesen u appsettings.json.");

        var users = sp.GetRequiredService<UserManager<AppUser>>();
        if (await users.FindByEmailAsync(email) is not null)
            return;   // admin vec postoji; lozinka se ne menja

        var password = cfg["Seed:AdminPassword"];   // User Secrets ili promenljiva okruzenja
        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Seed:AdminPassword nije podesen; upisite ga u User Secrets (MiniShop.Api).");

        var admin = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "Administrator"
        };

        // CreateAsync ne baca izuzetak kada lozinka ne prodje politiku - vraca neuspeh
        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("Admin nalog nije kreiran: " +
                string.Join("; ", created.Errors.Select(e => e.Description)));

        await users.AddToRoleAsync(admin, "Admin");
    }
}