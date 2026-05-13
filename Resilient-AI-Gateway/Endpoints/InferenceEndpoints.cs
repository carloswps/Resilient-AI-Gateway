using Resilient_AI_Gateway.Models;
using Resilient_AI_Gateway.Services;

namespace Resilient_AI_Gateway.Endpoints;

public static class InferenceEndpoints
{
    public static void MapInferenceEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/inference",
                async (InferenceRequest request, IGatewayService gatewayService, CancellationToken cancellationToken) =>
                {
                    var response = await gatewayService.ProcessAsync(request, cancellationToken);

                    return response.Error switch
                    {
                        null => Results.Ok(response),
                        "AllModelsUnavailable" => Results.Json(
                            response,
                            statusCode: StatusCodes.Status503ServiceUnavailable),

                        _ => Results.Json(
                            response,
                            statusCode: StatusCodes.Status500InternalServerError
                        )
                    };
                })
            .WithName("Inference")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get inference results";
                operation.Description = "Get inference results from a model";
                return Task.CompletedTask;
            });
    }
}