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
        var correlationId = GetOrGenerateCorrelationId(context);

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            await _next(context);
        }
    }

    private static string GetOrGenerateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString().Trim(); // use full value as-is
        }

        return Guid.NewGuid().ToString("N")[..8]; // short GUID only if generated
    }
}