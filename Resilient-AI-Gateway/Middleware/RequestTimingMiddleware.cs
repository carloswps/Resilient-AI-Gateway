using System.Diagnostics;

namespace Resilient_AI_Gateway.Middleware;

public class RequestTimingMiddleware
{
    private readonly ILogger<RequestTimingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            // Store the request start time and elapsed time in the context items
            context.Items["RequestStartTime"] = startTime;
            context.Items["RequestDurationMs"] = elapsedMs;

            if (elapsedMs > 500)
                _logger.LogWarning(
                    "Slow request: {Method} {Path} took {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs
                );
        }
    }
}
