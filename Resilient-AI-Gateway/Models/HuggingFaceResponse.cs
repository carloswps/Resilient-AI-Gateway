using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Models;

public class HuggingFaceResponse
{
    [JsonPropertyName("generated_text")]
    public string? GeneratedText { get; init; }
}