namespace MiniShop.Application.Abstractions;

/// <summary>
/// Skladiste fajlova. Implementacije: LocalDiskStorage (wwwroot/images) u razvoju,
/// blob storage u produkciji. Vraca URL pod kojim je fajl dostupan.
/// </summary>
public interface IFileStorage
{
    Task<string> SaveAsync(string name, Stream content, string contentType, CancellationToken ct);
}