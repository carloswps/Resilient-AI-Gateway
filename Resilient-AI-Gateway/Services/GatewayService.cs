using System.Diagnostics;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Exceptions;
using Resilient_AI_Gateway.Logging;
using Resilient_AI_Gateway.Models;
using Resilient_AI_Gateway.Resilience;

namespace Resilient_AI_Gateway.Services;

/// <summary>
///     Provides gateway services for interacting with external AI model providers.
///     This class processes inference requests, using a resilient strategy to handle
///     potential failures and ensure reliable communication with the AI models.
/// </summary>
public class GatewayService : IGatewayService
{
    private readonly IHuggingFaceClient _huggingFaceClient;
    private readonly ILogger<GatewayService> _logger;
    private readonly IRequestLogger _requestLogger;
    private readonly ResilienceOptions _resilienceOptions;

    public GatewayService(
        IHuggingFaceClient huggingFaceClient,
        ILogger<GatewayService> logger,
        ResilienceOptions resilienceOptions,
        IRequestLogger requestLogger)
    {
        _huggingFaceClient = huggingFaceClient;
        _logger = logger;
        _resilienceOptions = resilienceOptions;
        _requestLogger = requestLogger;
    }

    public async Task<InferenceResponse> ProcessAsync(
        InferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        var requestId = Guid.NewGuid().ToString();
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        var pipeline = ResiliencePipelineFactory.Create(
            _resilienceOptions, _logger, _huggingFaceClient);

        var chatRequest = new ChatCompletionRequest
        {
            Model = request.Model,
            Messages =
            [
                new ChatMessage
                {
                    Role = "user",
                    Content = request.Inputs
                }
            ],
            MaxTokens = request.Parameters?.MaxNewTokens,
            Temperature = request.Parameters?.Temperature,
            TopP = request.Parameters?.TopP
        };

        try
        {
            var response = await pipeline.ExecuteAsync(
                async ct =>
                {
                    var chatReqWithModel = new ChatCompletionRequest
                    {
                        Model = chatRequest.Model,
                        Messages = chatRequest.Messages,
                        MaxTokens = chatRequest.MaxTokens,
                        Temperature = chatRequest.Temperature,
                        TopP = chatRequest.TopP
                    };
                    return await _huggingFaceClient.CallChatCompletionAsync(
                        chatReqWithModel, ct);
                },
                cancellationToken);

            stopwatch.Stop();

            var chatResponse = await response.Content
                .ReadFromJsonAsync<ChatCompletionResponse>(
                    cancellationToken);

            var generatedText = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content;

            _requestLogger.Log(new RequestLogDocument
            {
                RequestId = requestId,
                Timestamp = startTime,
                Endpoint = "/api/v1/inference",
                HttpMethod = "POST",
                RequestedModel = request.Model,
                ModelUsed = request.Model,
                StatusCode = 200,
                LatencyMs = stopwatch.ElapsedMilliseconds,
                ResponseSizeBytes = generatedText?.Length
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
                HttpMethod = "POST",
                RequestedModel = request.Model,
                ModelUsed = request.Model,
                FallbackActivated = true,
                RetryAttempts = _resilienceOptions.MaxRetryAttempts,
                StatusCode = 503,
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