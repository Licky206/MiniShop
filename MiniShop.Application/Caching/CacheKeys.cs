namespace MiniShop.Application.Caching;

/// <summary>Jedino mesto na kom nastaju kljucevi i tagovi kesa.</summary>
public static class CacheKeys
{
    public const string CatalogTag = "catalog";
    public const string CatalogStamp = "catalog:v1:stamp";

    /// <summary>
    /// Svi parametri ProductQuery-ja. Stranica i velicina se normalizuju isto kao
    /// u EfProductRepository.GetPageAsync; pretraga je poslednja jer je slobodan tekst.
    /// </summary>
    public static string Catalog(ProductQuery q)
    {
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var sort = q.Sort ?? "name";

        // Invariant: decimalni separator ne sme zavisiti od jezika zahteva.
        // Dva poziva, jer "+" izmedju dva $"..." daje string, a ne FormattableString (CS1503).
        var filter = FormattableString.Invariant(
            $"c={q.CategoryId}:min={q.MinPrice}:max={q.MaxPrice}");
        return FormattableString.Invariant(
            $"catalog:v1:{filter}:s={sort}:p={page}:ps={pageSize}:q={q.Search}");
    }
}