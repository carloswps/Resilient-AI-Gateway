using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Shared.Contracts;

public class ModelFallbackList
{
    [JsonPropertyName("primary_model")]
    public string PrimaryModel { get; init; } = string.Empty;

    [JsonPropertyName("fallback_models")]
    public IReadOnlyList<string> FallbackModels { get; init; } = Array.Empty<string>();
}