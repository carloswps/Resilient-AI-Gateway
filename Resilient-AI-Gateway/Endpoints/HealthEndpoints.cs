using Microsoft.Extensions.Diagnostics.HealthChecks;
using Resilient_AI_Gateway.Services;

namespace Resilient_AI_Gateway.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    status = "Healthy",
                    timestamp = DateTime.UtcNow
                });
            })
            .WithName("Liveness")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Liveness check";
                operation.Description = "Basic liveness check — always returns healthy if the process is running.";
                return Task.CompletedTask;
            });

        app.MapGet("/health/ready", async (GatewayHealthCheck healthCheck) =>
            {
                var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

                var response = new
                {
                    status = result.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks = result.Data.ToDictionary(kv => kv.Key, kv => kv.Value)
                };

                return result.Status == HealthStatus.Healthy
                    ? Results.Ok(response)
                    : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("Readiness")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Readiness check";
                operation.Description = "Checks if MongoDB and Hugging Face API are reachable.";
                return Task.CompletedTask;
            });
    }
}
