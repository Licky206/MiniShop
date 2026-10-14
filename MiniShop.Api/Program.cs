var builder = WebApplication.CreateBuilder(args);

// jezik poruka iz .resx: do nedelje 7 fiksno srpski; tada ga bira Accept-Language.
// Menja se samo UI kultura (tekstovi), ne i format brojeva i datuma.
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture =
    System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS");

// 1) registracija servisa u DI kontejner
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services
    .AddControllers(o => o.Filters.Add<AuditFilter>())       // audit za sve akcije
    .AddJsonOptions(o =>
    {
        var j = o.JsonSerializerOptions;
        j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        j.Converters.Add(new JsonStringEnumConverter());       // enum kao string
        j.NumberHandling = JsonNumberHandling.Strict;          // "4990" nije broj
    })
    .AddDataAnnotationsLocalization(o =>
        // svi DTO-ovi dele jedan skup prevoda: Resources/Dtos.ProductCreateDto.*.resx
        o.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(ProductCreateDto)));

builder.Services.AddOpenApi();

// Location: /api/products/6, a ne /api/Products/6 (ime kontrolera)
builder.Services.AddRouting(o => o.LowercaseUrls = true);

// greske: ProblemDetails + globalni handler (Errors/GlobalExceptionHandler.cs)
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// konfiguracija: sekcija Catalog -> CatalogOptions, provera pri startu
builder.Services.AddOptions<CatalogOptions>()
    .Bind(builder.Configuration.GetSection(CatalogOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// katalog: lista je u memoriji -> Singleton; od nedelje 4 AddScoped (DbContext)
builder.Services.AddSingleton<IProductService, InMemoryProductService>();

var app = builder.Build();

// 2) sastavljanje middleware pipeline-a (redosled = redosled izvrsavanja)
app.UseMiddleware<RequestTiming>();        // prvi: meri i sve ispod sebe

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();       // detalji greske samo u razvoju
    app.MapOpenApi();                      // /openapi/v1.json
    app.MapScalarApiReference();           // /scalar/v1
}
else
{
    app.UseExceptionHandler();             // GlobalExceptionHandler -> problem+json
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
