using Polly;
using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Resilience;

public static class ResilienceContextExtensions
{
    private static readonly ResiliencePropertyKey<HuggingFaceRequest> RequestKey =
        new("HuggingFaceRequest");

    public static void SetRequest(this ResilienceContext context, HuggingFaceRequest request)
    {
        context.Properties.Set(RequestKey, request);
    }

    public static HuggingFaceRequest? GetRequest(this ResilienceContext context)
    {
        return context.Properties.TryGetValue(RequestKey, out var request) ? request : null;
    }
}