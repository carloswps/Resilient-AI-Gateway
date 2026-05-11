using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Models;

public class InferenceResponse
{
    [JsonPropertyName("request_id")]
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    [JsonPropertyName("model_used")]
    public string ModelUsed { get; init; } = string.Empty;

    [JsonPropertyName("fallback_activated")]
    public bool FallbackActivated { get; init; }

    [JsonPropertyName("generated_text")]
    public string? GeneratedText { get; init; }

    [JsonPropertyName("latency_ms")]
    public long LatencyMs { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("retry_after_seconds")]
    public int? RetryAfterSeconds { get; init; }
}