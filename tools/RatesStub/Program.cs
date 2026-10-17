// Lokalni stub spoljnog servisa kursa: GET /latest?base=RSD
// Rezim se menja u toku rada: POST /mode/ok | /mode/fail | /mode/slow
var app = WebApplication.Create(args);
var mode = "ok";

app.MapGet("/latest", async (HttpRequest req, CancellationToken ct) =>
{
    if (req.Headers["X-Api-Key"] != "dev-key")
        return Results.Unauthorized();
    if (mode == "fail")
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    if (mode == "slow")
        await Task.Delay(TimeSpan.FromSeconds(5), ct);

    return Results.Ok(new
    {
        @base = "RSD",
        date = DateOnly.FromDateTime(DateTime.UtcNow),
        rates = new Dictionary<string, decimal> { ["EUR"] = 0.00853m, ["USD"] = 0.00925m }
    });
});

app.MapPost("/mode/{m}", (string m) =>
{
    mode = m;
    return Results.Ok(new { mode });
});

app.Run("http://localhost:5299");