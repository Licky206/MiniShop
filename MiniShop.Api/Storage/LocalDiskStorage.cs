namespace MiniShop.Api.Storage;

/// <summary>Cuva fajlove u wwwroot/images i vraca relativan URL /images/{ime}. Singleton.</summary>
public sealed class LocalDiskStorage : IFileStorage
{
    private readonly string _root;

    public LocalDiskStorage(IWebHostEnvironment env)
    {
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _root = Path.Combine(webRoot, "images");
        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(string name, Stream content, string contentType,
        CancellationToken ct)
    {
        // odbrana u dubinu: ime pravi pozivalac (GUID), ali putanju ovde ne prihvatamo
        var safe = Path.GetFileName(name);
        if (safe.Length == 0 || safe != name)
            throw new ArgumentException("Ime fajla ne sme da sadrzi putanju.", nameof(name));

        var path = Path.Combine(_root, safe);
        await using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
            FileShare.None, bufferSize: 81920, useAsync: true);
        await content.CopyToAsync(file, ct);

        // contentType ovde nije potreban; blob implementacija ga upisuje uz fajl
        return $"/images/{safe}";
    }
}