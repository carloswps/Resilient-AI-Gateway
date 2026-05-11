using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public interface IHuggingFaceClient
{
    public Task<HttpResponseMessage> CallModelAsync(
        string modelId,
        HuggingFaceRequest request,
        CancellationToken cancellationToken = default
    );
}