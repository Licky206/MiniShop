namespace MiniShop.Application.Caching;

/// <summary>
/// Pecat kataloga za ETag: trenutak poslednje izmene i broj proizvoda.
/// Broj je potreban jer brisanje ne menja MAX(UpdatedAt).
/// </summary>
public readonly record struct CatalogStamp(DateTime LastChange, int Count)
{
    // slab ETag: predstavlja verziju podataka, ne bajtove tela
    public string ToETag() =>
        FormattableString.Invariant($"W/\"{LastChange:yyyyMMddHHmmssfff}-{Count}\"");
}