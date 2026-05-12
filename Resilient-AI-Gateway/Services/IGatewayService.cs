using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public interface IGatewayService
{
    Task<InferenceResponse> ProcessAsync(InferenceRequest request, CancellationToken cancellationToken = default);
}