namespace MiniShop.Api.Middleware;

// konvencionalni middleware: konstruktor se poziva jednom, InvokeAsync po zahtevu
public class RequestTiming(RequestDelegate next, ILogger<RequestTiming> log)
{
	public async Task InvokeAsync(HttpContext ctx)
	{
		var sw = Stopwatch.StartNew();
		try
		{
			await next(ctx);                  // predaj sledecem u lancu
		}
		finally
		{
			sw.Stop();
			log.LogInformation(
				"{Method} {Path} -> {Status} za {Elapsed} ms",
				ctx.Request.Method, ctx.Request.Path,
				ctx.Response.StatusCode, sw.ElapsedMilliseconds);
		}
	}
}
