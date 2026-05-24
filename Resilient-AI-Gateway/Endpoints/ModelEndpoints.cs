using Resilient_AI_Gateway.Services;

namespace Resilient_AI_Gateway.Endpoints;

public static class ModelEndpoints
{
    public static void MapModelEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/models", async (IModelService modelService, CancellationToken cancellationToken) =>
        {
            var models = await modelService.GetModelsAsync(cancellationToken);
            return Results.Ok(models);
        }).WithName("ListModels").AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            operation.Summary = "List available models";
            operation.Description = "List available models from Hugging Face router API";
            return Task.CompletedTask;
        });
    }
}