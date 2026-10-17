namespace MiniShop.Api.Options;

/// <summary>Sekcija "Rates": adresa servisa kursa; ApiKey dolazi iz User Secrets.</summary>
public sealed class RatesOptions
{
    public const string Section = "Rates";

    [Required, Url]
    public string BaseUrl { get; init; } = "";

    public string ApiKey { get; init; } = "";
}