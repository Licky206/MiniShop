namespace MiniShop.Tests;

/// <summary>
/// Potpisuje access token istim TokenService-om kao AuthController, sa kljucem
/// iz appsettings.Testing.json - API ga prihvata kao da je izdat pri prijavi.
/// </summary>
public static class TestJwt
{
    public static readonly string SettingsPath =
        Path.Combine(AppContext.BaseDirectory, "appsettings.Testing.json");

    private static readonly JwtOptions Settings = new ConfigurationBuilder()
        .AddJsonFile(SettingsPath, optional: false)
        .Build()
        .GetSection("Jwt")
        .Get<JwtOptions>()!;

    public static string For(string email, string role = "Customer")
    {
        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            UserName = email
        };
        var tokens = new TokenService(Options.Create(Settings));
        var (token, _) = tokens.Create(user, [role]);
        return token;
    }
}