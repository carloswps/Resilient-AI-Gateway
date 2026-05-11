using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Models;

public class InferenceRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("inputs")]
    public string Inputs { get; init; } = string.Empty;

    [JsonPropertyName("parameters")]
    public InferenceParameters? Parameters { get; init; }
}

public class InferenceParameters
{
    [JsonPropertyName("max_new_tokens")]
    public int? MaxNewTokens { get; init; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }
}