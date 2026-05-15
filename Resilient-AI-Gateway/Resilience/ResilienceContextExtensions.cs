using Polly;
using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Resilience;

public static class ResilienceContextExtensions
{
    private static readonly ResiliencePropertyKey<ChatCompletionRequest> RequestKey =
        new("ChatCompletionRequest");

    public static void SetRequest(this ResilienceContext context, ChatCompletionRequest request)
    {
        context.Properties.Set(RequestKey, request);
    }

    public static ChatCompletionRequest? GetRequest(this ResilienceContext context)
    {
        return context.Properties.TryGetValue(RequestKey, out var request) ? request : null;
    }
}