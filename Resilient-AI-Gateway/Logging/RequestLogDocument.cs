using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Logging;

public class RequestLogDocument
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("client_id")]
    public string ClientId { get; init; } = string.Empty;

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; init; } = string.Empty;

    [JsonPropertyName("http_method")]
    public string HttpMethod { get; init; } = string.Empty;

    [JsonPropertyName("requested_model")]
    public string RequestedModel { get; init; } = string.Empty;

    [JsonPropertyName("model_used")]
    public string ModelUsed { get; init; } = string.Empty;

    [JsonPropertyName("fallback_activated")]
    public bool FallbackActivated { get; init; }

    [JsonPropertyName("retry_attempts")]
    public int RetryAttempts { get; init; }

    [JsonPropertyName("status_code")]
    public int StatusCode { get; init; }

    [JsonPropertyName("latency_ms")]
    public long LatencyMs { get; init; }

    [JsonPropertyName("hf_latency_ms")]
    public long? HfLatencyMs { get; init; }

    [JsonPropertyName("payload_size_bytes")]
    public long? PayloadSizeBytes { get; init; }

    [JsonPropertyName("response_size_bytes")]
    public long? ResponseSizeBytes { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }
}