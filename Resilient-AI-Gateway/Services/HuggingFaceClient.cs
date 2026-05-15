using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

/// <summary>
///     Represents a client for interacting with Hugging Face's inference API.
///     Provides functionality to send inference requests to specific models hosted on Hugging Face.
/// </summary>
public class HuggingFaceClient : IHuggingFaceClient
{
    private readonly HttpClient _httpClient;

    public HuggingFaceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> CallChatCompletionAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "chat/completions",
            request,
            cancellationToken
        );

        return response;
    }
}