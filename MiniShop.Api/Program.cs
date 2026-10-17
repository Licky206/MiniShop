using MiniShop.Application;


var builder = WebApplication.CreateBuilder(args);

// jezik poruka iz .resx: do nedelje 7 fiksno srpski; tada ga bira Accept-Language.
// Menja se samo UI kultura (tekstovi), ne i format brojeva i datuma.

//TRECA NEDELJA
//System.Globalization.CultureInfo.DefaultThreadCurrentUICulture =
//   System.Globalization.CultureInfo.GetCultureInfo("sr-Latn-RS");

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

builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;                 // zaglavlje api-supported-versions
        o.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";               // 1.0 -> "v1" = ime OpenAPI dokumenta
        o.SubstituteApiVersionInUrl = true;         // u dokumentu /api/v1/..., ne v{version}
    });

// OpenAPI dokument "v1" (isto ime kao grupa iz AddApiExplorer) sa opisom i Bearer semom
builder.Services.AddOpenApi("v1", o =>
{
    o.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info = new OpenApiInfo
        {
            Title = "MiniShop API",
            Version = "v1",
            Description = "Katalog proizvoda, kategorije i narudzbenice."
        };

        // .NET 10 (Microsoft.OpenApi 2.x): Components i SecuritySchemes mogu biti null,
        // a vrednosti recnika su IOpenApiSecurityScheme (.NET 9 / 1.x: OpenApiSecurityScheme)
        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        doc.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT pristupni token, bez prefiksa Bearer (izdaje se od nedelje 10)."
        };
        return Task.CompletedTask;
    });
});

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


builder.Services
    .AddDataLayer(builder.Configuration, builder.Environment.IsDevelopment())
    .AddApplicationLayer();

builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

var cultures = new[] { "sr-Latn-RS", "sr-Cyrl-RS", "en" };
builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    o.SetDefaultCulture("sr-Latn-RS")
     .AddSupportedCultures(cultures)       // formatiranje brojeva i datuma
     .AddSupportedUICultures(cultures);    // izbor .resx fajla
    o.ApplyCurrentCultureToResponseHeaders = true;   // Content-Language u odgovoru
    o.FallBackToParentUICultures = true;             // en-GB -> en
    // API ne pamti jezik izmedju zahteva: bez cookie provajdera
    o.RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider(),          // ?culture=en&ui-culture=en
        new AcceptLanguageHeaderRequestCultureProvider()  // Accept-Language: en
    ];
});

var app = builder.Build();

// u razvoju uvek; u produkciji samo ako je eksplicitno ukljuceno
// (u compose-u, nedelja 14: Database__MigrateOnStartup=true)
if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await app.Services.MigrateDatabaseAsync();
}

// 2) sastavljanje middleware pipeline-a (redosled = redosled izvrsavanja)
app.UseMiddleware<RequestTiming>();        // prvi: meri i sve ispod sebe

app.UseExceptionHandler();        // uvek: 400 za ValidationException, 500 za ostalo
app.UseRequestLocalization();     // pre MapControllers: akcija vidi kulturu zahteva

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();             // /openapi/v1.json
    app.MapScalarApiReference();  // /scalar/v1
}

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