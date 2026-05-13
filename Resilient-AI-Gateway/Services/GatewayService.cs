using System.Diagnostics;
using Polly;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Exceptions;
using Resilient_AI_Gateway.Logging;
using Resilient_AI_Gateway.Models;
using Resilient_AI_Gateway.Resilience;

namespace Resilient_AI_Gateway.Services;

/// <summary>
/// Provides gateway services for interacting with external AI model providers.
/// This class processes inference requests, using a resilient strategy to handle
/// potential failures and ensure reliable communication with the AI models.
/// </summary>
public class GatewayService : IGatewayService
{
    private readonly IHuggingFaceClient _huggingFaceClient;
    private readonly ILogger<GatewayService> _logger;
    private readonly ResilienceOptions _resilienceOptions;
    private readonly IRequestLogger _requestLogger;

    public GatewayService(IHuggingFaceClient huggingFaceClient, 
        ILogger<GatewayService> logger,
        ResilienceOptions resilienceOptions, 
        IRequestLogger requestLogger)
    {
        _huggingFaceClient = huggingFaceClient;
        _logger = logger;
        _resilienceOptions = resilienceOptions;
        _requestLogger = requestLogger;
    }

    public async Task<InferenceResponse> ProcessAsync(InferenceRequest request, CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        var pipeline = ResiliencePipelineFactory.Create(_resilienceOptions, _logger, _huggingFaceClient);

        var hfRequest = new HuggingFaceRequest
        {
            Inputs = request.Inputs,
            Parameters = request.Parameters
        };

        try
        {
            var response = await pipeline.ExecuteAsync(async ct => await _huggingFaceClient.CallModelAsync(
                request.Model, hfRequest, ct), cancellationToken);

            stopwatch.Stop();

            var hfResponse =
                await response.Content.ReadFromJsonAsync<HuggingFaceResponse[]>(cancellationToken: cancellationToken);

            var generatedText = hfResponse?.FirstOrDefault()?.GeneratedText;

            _requestLogger.Log(new RequestLogDocument
            {
                RequestId = requestId,
                Timestamp = startTime,
                Endpoint = "/api/v1/inference",
                HttpMethod = "Post",
                RequestedModel = request.Model,
                ModelUsed = request.Model,
                StatusCode = 200,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                ResponseSizeBytes = generatedText?.Length ?? 0
            });

            return new InferenceResponse
            {
                RequestId = requestId,
                ModelUsed = request.Model,
                GeneratedText = generatedText,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (AllModelsUnavailableException ex)
        {
            stopwatch.Stop();

            _requestLogger.Log(new RequestLogDocument
            {
                RequestId = requestId,
                Timestamp = startTime,
                Endpoint = "/api/v1/inference",
                HttpMethod = "Post",
                RequestedModel = request.Model,
                ModelUsed = request.Model,
                StatusCode = 200,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            });

            return new InferenceResponse
            {
                RequestId = requestId,
                Error = "AllModelsUnavailable",
                Message = "Todos os modelos configurados estão indisponíveis no momento.",
                RetryAfterSeconds = 30,
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _requestLogger.Log(new RequestLogDocument
            {
                RequestId = requestId,
                Timestamp = startTime,
                Endpoint = "/api/v1/inference",
                HttpMethod = "POST",
                RequestedModel = request.Model,
                StatusCode = 500,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            });

            return new InferenceResponse
            {
                RequestId = requestId,
                Error = "InternalError",
                Message = "Ocorreu um erro interno ao processar a requisição.",
                LatencyMs = stopwatch.ElapsedMilliseconds
            };
        }
    }
}