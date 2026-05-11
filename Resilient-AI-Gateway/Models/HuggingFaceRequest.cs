using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Models;

public class HuggingFaceRequest
{
    [JsonPropertyName("inputs")]
    public string Inputs { get; init; } = string.Empty;

    [JsonPropertyName("parameters")]
    public InferenceParameters? Parameters { get; init; }
}