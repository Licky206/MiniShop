var builder = WebApplication.CreateBuilder(args);

// 1) registracija servisa u DI kontejner
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 2) sastavljanje middleware pipeline-a (redosled je bitan!)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();              // JSON opis na /openapi/v1.json
    // paket Scalar.AspNetCore -> UI na /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
