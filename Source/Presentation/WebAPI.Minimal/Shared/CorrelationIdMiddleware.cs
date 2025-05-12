namespace WebAPI.Minimal.Shared;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Try to get it from incoming header or generate a new one
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        // Enrich Serilog context for all logs during this request
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Optional: return it in the response
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            await _next(context);
        }
    }
}