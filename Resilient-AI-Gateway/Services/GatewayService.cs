using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public class GatewayService : IGatewayService
{
    private readonly IHuggingFaceClient _huggingFaceClient;
    private readonly ILogger<GatewayService> _logger;
    private readonly ResilienceOptions _resilienceOptions;

    public GatewayService(IHuggingFaceClient huggingFaceClient, ILogger<GatewayService> logger,
        ResilienceOptions resilienceOptions)
    {
        _huggingFaceClient = huggingFaceClient;
        _logger = logger;
        _resilienceOptions = resilienceOptions;
    }

    public Task<InferenceResponse> ProcessAsync(InferenceRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}