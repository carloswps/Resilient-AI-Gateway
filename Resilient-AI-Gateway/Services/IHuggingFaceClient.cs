using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public interface IHuggingFaceClient
{
    Task<HttpResponseMessage> CallChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default
    );
}