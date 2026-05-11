using System.Text.Json;
using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public class HuggingFaceClient : IHuggingFaceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public HuggingFaceClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public async Task<HttpResponseMessage> CallModelAsync(string modelId, HuggingFaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            modelId,
            request,
            _jsonSerializerOptions,
            cancellationToken
        );

        return response;
    }
}