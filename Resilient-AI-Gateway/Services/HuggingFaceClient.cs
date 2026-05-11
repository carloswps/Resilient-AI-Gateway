using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public class HuggingFaceClient : IHuggingFaceClient
{
    public Task<HttpResponseMessage> CallModelAsync(string modelId, HuggingFaceRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}