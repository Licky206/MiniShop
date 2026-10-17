using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using MiniShop.Application;

var builder = WebApplication.CreateBuilder(args);

// ---------- osnovno ----------
builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

builder.Services
    .AddControllers(o => o.Filters.Add<AuditFilter>())
    .AddJsonOptions(o =>
    {
        var j = o.JsonSerializerOptions;
        j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        j.Converters.Add(new JsonStringEnumConverter());
        j.NumberHandling = JsonNumberHandling.Strict;
    });

builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
        o.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });

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

        doc.Components ??= new OpenApiComponents();
        doc.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        doc.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description =
                    "JWT pristupni token, bez prefiksa Bearer."
            };

        return Task.CompletedTask;
    });

    // ---------- nedelja 11: autorizacija u OpenAPI ----------
    // Svaka operacija dobija red "Autorizacija: <pravilo>"
    // izveden iz EndpointMetadata.
    o.AddOperationTransformer((operation, context, ct) =>
    {
        var metadata =
            context.Description.ActionDescriptor.EndpointMetadata;

        string rule;

        if (metadata.OfType<IAllowAnonymous>().Any())
        {
            rule = "javno, bez tokena";
        }
        else
        {
            var policies = metadata
                .OfType<IAuthorizeData>()
                .Select(a => a.Policy ?? a.Roles ?? "prijava")
                .Distinct()
                .ToList();

            rule = policies.Count == 0
                ? "prijava (FallbackPolicy)"
                : string.Join(" + ", policies);
        }

        operation.Description =
            $"{operation.Description}\n\nAutorizacija: {rule}".Trim();

        return Task.CompletedTask;
    });
});

builder.Services.AddRouting(o => o.LowercaseUrls = true);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOptions<CatalogOptions>()
    .Bind(builder.Configuration.GetSection(CatalogOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ---------- slojevi ----------
builder.Services
    .AddDataLayer(
        builder.Configuration,
        builder.Environment.IsDevelopment())
    .AddApplicationLayer();

// ---------- lokalizacija (nedelja 7) ----------
var cultures = new[] { "sr-Latn-RS", "sr-Cyrl-RS", "en" };

builder.Services.Configure<RequestLocalizationOptions>(o =>
{
    o.SetDefaultCulture("sr-Latn-RS")
        .AddSupportedCultures(cultures)
        .AddSupportedUICultures(cultures);

    o.ApplyCurrentCultureToResponseHeaders = true;
    o.FallBackToParentUICultures = true;

    o.RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

// ---------- nedelja 8: spoljni servis kursa ----------
builder.Services.AddOptions<RatesOptions>()
    .Bind(builder.Configuration.GetSection(RatesOptions.Section))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ApiKeyHandler>();

var ratesClient = builder.Services.AddHttpClient<ExchangeRateClient>((sp, c) =>
{
    var rates = sp.GetRequiredService<IOptions<RatesOptions>>().Value;

    c.BaseAddress = new Uri(rates.BaseUrl);
    c.Timeout = TimeSpan.FromSeconds(10);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

ratesClient.AddResilienceHandler("rates", b =>
{
    b.AddTimeout(TimeSpan.FromSeconds(4));

    b.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 2,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    });

    b.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 5,
        BreakDuration = TimeSpan.FromSeconds(15)
    });

    b.AddTimeout(TimeSpan.FromSeconds(1));
});

ratesClient.AddHttpMessageHandler<ApiKeyHandler>();

builder.Services.AddScoped<IExchangeRateProvider, CachedExchangeRateProvider>();

// ---------- nedelja 8: CORS ----------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(o =>
{
    o.AddPolicy("spa", policy => policy
        .WithOrigins(allowedOrigins)
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders(
            "Authorization",
            "Content-Type",
            "X-Api-Version",
            "X-Requested-With",
            "X-SignalR-User-Agent")
        .WithExposedHeaders("X-Pagination", "Location")
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

    o.AddPolicy("dev", policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders("X-Pagination", "Location")
        .AllowCredentials());
});

// ---------- nedelja 8: fajlovi i SignalR ----------
builder.Services.AddSingleton<IFileStorage, LocalDiskStorage>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IOrderNotifier, SignalROrderNotifier>();

// ---------- nedelja 9: Identity ----------
builder.Services
    .AddIdentityCore<AppUser>(o =>
    {
        o.Password.RequiredLength = 10;
        o.Password.RequireNonAlphanumeric = false;
        o.User.RequireUniqueEmail = true;
        o.SignIn.RequireConfirmedEmail = false;
        o.Lockout.MaxFailedAccessAttempts = 5;
        o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// ---------- nedelja 10: JWT ----------

// JwtOptions se ucitava iz:
// - appsettings.json: Issuer, Audience, AccessTokenMinutes, RefreshTokenDays
// - User Secrets: Jwt:Key
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        o => Encoding.UTF8.GetByteCount(o.Key) >= 32,
        "Jwt:Key mora imati najmanje 32 bajta (HS256).")
    .ValidateOnStart();

// Servisi za access i refresh tokene.
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<RefreshTokenService>();

// Bearer je podrazumevana authentication sema.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

// Parametri za validaciju JWT access tokena.
builder.Services
    .AddOptions<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((o, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;

        // sub -> ClaimTypes.NameIdentifier
        // role -> ClaimTypes.Role
        o.MapInboundClaims = true;

        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt.Key)),

            ValidAlgorithms =
            [
                SecurityAlgorithms.HmacSha256
            ],

            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwt.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Email
        };

        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                string? token =
                    ctx.Request.Query["access_token"];

                // Query-string token dozvoljavamo samo SignalR hubovima.
                if (!string.IsNullOrEmpty(token) &&
                    ctx.Request.Path.StartsWithSegments("/hubs"))
                {
                    ctx.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

// ---------- nedelja 11: autorizacija ----------

// Zatvoreno po podrazumevanom + imenovane politike.
builder.Services.AddAuthorization(options =>
{
    // [Authorize] bez imena politike: validan Bearer token
    options.DefaultPolicy =
        new AuthorizationPolicyBuilder(
            JwtBearerDefaults.AuthenticationScheme)
            .RequireAuthenticatedUser()
            .Build();

    // Endpoint BEZ ikakvog atributa: isto sto i [Authorize].
    options.FallbackPolicy = options.DefaultPolicy;

    // Samo korisnici sa Admin rolom.
    options.AddPolicy("AdminOnly", p =>
        p.RequireRole("Admin"));

    // Vlasnik resursa ili Admin.
    options.AddPolicy("SameOwner", p =>
        p.Requirements.Add(new SameOwnerRequirement()));
});

// Handler nema zavisnosti -> Singleton.
builder.Services.AddSingleton<IAuthorizationHandler, OrderOwnerHandler>();

var app = builder.Build();

// ---------- baza i Identity seed ----------
if (app.Environment.IsDevelopment() ||
    app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    // Prvo migracije.
    await app.Services.MigrateDatabaseAsync();

    // Zatim Identity seed.
    using var scope = app.Services.CreateScope();

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        app.Configuration);
}

app.UseMiddleware<RequestTiming>();

app.UseExceptionHandler();

app.UseRequestLocalization();

// OpenAPI i Scalar moraju ostati javno dostupni,
// iako je FallbackPolicy sada ukljucena.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();

// Static files moraju biti PRE UseAuthorization().
// Tako /images/... ostaje javno dostupan.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff"
});

app.UseRouting();

app.UseCors(
    app.Environment.IsDevelopment()
        ? "dev"
        : "spa");

// Authentication mora biti pre Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/orders");

app.Run();