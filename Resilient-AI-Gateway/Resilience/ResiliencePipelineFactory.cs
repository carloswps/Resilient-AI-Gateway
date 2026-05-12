using System.Net;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Exceptions;
using Resilient_AI_Gateway.Services;

namespace Resilient_AI_Gateway.Resilience;

public static class ResiliencePipelineFactory
{
    public static ResiliencePipeline<HttpResponseMessage> Create(
        ResilienceOptions options,
        ILogger logger,
        IHuggingFaceClient huggingFaceClient
    )
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddTimeout(TimeSpan.FromSeconds(options.GlobalTimeoutSeconds))
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(options.BaseDelayMs),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>()
                    .HandleResult(r => r.StatusCode is
                        HttpStatusCode.TooManyRequests or
                        HttpStatusCode.ServiceUnavailable or
                        HttpStatusCode.GatewayTimeout),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry {Attempt}/{Max} após {Delay}ms. Reason: {Reason}",
                        args.AttemptNumber + 1,
                        options.MaxRetryAttempts,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message
                        ?? args.Outcome.Result?.StatusCode.ToString());
                    return default;
                }
            })
            .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<Exception>()
                    .HandleResult(r => !r.IsSuccessStatusCode),

                FallbackAction = async args =>
                {
                    foreach (var fallbackModel in options.FallbackModels)
                    {
                        logger.LogWarning("Tentando modelo fallback: {Model}", fallbackModel);

                        var request = args.Context.GetRequest();
                        var fallbackResponse = await huggingFaceClient.CallModelAsync(
                            fallbackModel,
                            request,
                            args.Context.CancellationToken
                        );

                        if (fallbackResponse.IsSuccessStatusCode)
                            return Outcome.FromResult(fallbackResponse);
                    }

                    throw new AllModelsUnavailableException(
                        "Todos os modelos configurados estão indisponíveis.");
                }
            })
            .Build();
    }
}