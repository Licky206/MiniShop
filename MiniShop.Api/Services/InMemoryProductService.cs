namespace MiniShop.Api.Services;

// Singleton: lista zivi u memoriji procesa i od nedelje 3 se i MENJA.
// Vise zahteva radi paralelno na vise niti, pa svaki pristup ide pod lock.
// Od nedelje 4 zamenjuje ga ProductService nad AppDbContext-om (Scoped).
public sealed class InMemoryProductService(ILogger<InMemoryProductService> log)
    : IProductService
{
    // interni red: cuva i polja koja ProductDto nema
    private sealed record Row(int Id, string Name, string Sku, string? Description,
        decimal Price, int StockQuantity, int CategoryId, bool IsActive);

    // iste kategorije koje u nedelji 4 postaju seed u bazi
    private static readonly Dictionary<int, string> _categories = new()
    {
        [1] = "Periferija",
        [2] = "Monitori",
        [3] = "Laptopovi"
    };

    private static readonly List<Row> _rows =
    [
        new(1, "Tastatura", "KB-100", "Mehanicka, US raspored", 4990m, 12, 1, true),
        new(2, "Mis", "MS-200", null, 2490m, 30, 1, true),
        new(3, "Monitor 27", "MN-270", "IPS, 1440p", 32990m, 5, 2, true),
        new(4, "USB-C kabl", "CB-USBC", null, 990m, 100, 1, true),
        new(5, "Laptop 14", "LT-140", "16 GB RAM, 512 GB SSD", 89990m, 3, 3, true)
    ];

    private static readonly Lock _gate = new();   // C# 13: System.Threading.Lock
    private static int _nextId = 6;

    // ct: potpis je pisan za bazu (nedelja 4); lista u memoriji ga ne koristi
    public Task<IReadOnlyList<ProductDto>> GetAllAsync(ProductQuery q, CancellationToken ct)
    {
        lock (_gate)
        {
            IEnumerable<Row> query = _rows.Where(r => r.IsActive);

            var search = q.Search;
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(r =>
                    r.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (q.CategoryId is int categoryId)
                query = query.Where(r => r.CategoryId == categoryId);
            if (q.MinPrice is decimal min)
                query = query.Where(r => r.Price >= min);
            if (q.MaxPrice is decimal max)
                query = query.Where(r => r.Price <= max);

            query = q.Sort switch
            {
                "price" => query.OrderBy(r => r.Price),
                "price_desc" => query.OrderByDescending(r => r.Price),
                "name_desc" => query.OrderByDescending(r => r.Name),
                _ => query.OrderBy(r => r.Name)
            };

            // ToList() pod lock-om: van njega lista moze da se promeni
            IReadOnlyList<ProductDto> page = query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(ToDto)
                .ToList();

            log.LogDebug("Katalog: {Count} proizvoda na strani {Page}", page.Count, q.Page);
            return Task.FromResult(page);
        }
    }

    public Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        lock (_gate)
        {
            var row = _rows.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(row is null ? null : ToDto(row));
        }
    }

    public Task<ProductDto?> CreateAsync(ProductCreateDto dto, CancellationToken ct)
    {
        lock (_gate)
        {
            if (_rows.Any(r => string.Equals(r.Sku, dto.Sku, StringComparison.OrdinalIgnoreCase)))
            {
                log.LogWarning("Pokusaj kreiranja proizvoda sa zauzetim SKU {Sku}", dto.Sku);
                return Task.FromResult<ProductDto?>(null);
            }

            // Id odredjuje server, nikad klijent (zastita od over-postinga)
            var row = new Row(_nextId++, dto.Name, dto.Sku, dto.Description,
                dto.Price, dto.StockQuantity, dto.CategoryId, IsActive: true);
            _rows.Add(row);

            log.LogInformation("Kreiran proizvod {Id} ({Sku})", row.Id, row.Sku);
            return Task.FromResult<ProductDto?>(ToDto(row));
        }
    }

    public Task<ProductDto?> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken ct)
    {
        lock (_gate)
        {
            var index = _rows.FindIndex(r => r.Id == id);
            if (index < 0) return Task.FromResult<ProductDto?>(null);

            // Sku se ne menja izmenom (isto kao ApplyUpdate u nedelji 4)
            var row = _rows[index] with
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                IsActive = dto.IsActive
            };
            _rows[index] = row;
            return Task.FromResult<ProductDto?>(ToDto(row));
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        lock (_gate)
        {
            return Task.FromResult(_rows.RemoveAll(r => r.Id == id) > 0);
        }
    }

    // nepoznata kategorija daje prazno ime (isto kao ToDto u nedelji 4);
    // proveru postojanja kategorije uvodimo u nedelji 6 (category-not-found)
    private static ProductDto ToDto(Row r) =>
        new(r.Id, r.Name, r.Sku, r.Price, r.StockQuantity,
            _categories.GetValueOrDefault(r.CategoryId, ""));
}
