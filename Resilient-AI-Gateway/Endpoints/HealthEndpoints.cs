namespace Resilient_AI_Gateway.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () =>
            {
                return Results.Ok(new
                {
                    Status = "Healthy",
                    Timestamp = DateTime.UtcNow
                });
            })
            .WithName("Health")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Health check";
                operation.Description = "Health check endpoint";
                return Task.CompletedTask;
            });
        
        app.MapGet("/healh/ready", () =>
        {
            // TODO: implements after correct health check for (Db Ping, HF connectivity).
            return Results.Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                checks= new
                {
                    database = "Healthy",
                    huggingface_connectivity = "Healthy"
                }
                
            });
        })
            .WithName("Readiness")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Readiness check";
                operation.Description = "Readiness check endpoint";
                return Task.CompletedTask;
            });
    }
}