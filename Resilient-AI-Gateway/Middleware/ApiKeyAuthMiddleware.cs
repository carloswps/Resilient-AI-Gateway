using System.Net;
using Microsoft.Extensions.Options;
using Resilient_AI_Gateway.Configuration;

namespace Resilient_AI_Gateway.Middleware;

public class ApiKeyAuthMiddleware
{
    private static readonly PathString[] ExcludePaths =
    {
        "/",
        "/health",
        "/health/ready",
        "/health/live",
        "/openapi",
        "/scalar",
        "/api/v1/models"
    };


    private readonly RequestDelegate _next;
    private readonly GatewayOptions _options;

    public ApiKeyAuthMiddleware(RequestDelegate next, IOptions<GatewayOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ExcludePaths.Any(p => context.Request.Path.StartsWithSegments(p)))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Gateway-Key", out var extractedApiKey)
            || string.IsNullOrWhiteSpace(extractedApiKey)
           )
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Missing or invalid API key"
            });
            return;
        }

        var apiKey = extractedApiKey.ToString();
        if (!_options.ApiKeys.Contains(apiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unauthorized",
                message = "Invalid API key"
            });
            return;
        }

        context.Items["ClientId"] = apiKey;
        await _next(context);
    }
}