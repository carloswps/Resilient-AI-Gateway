using System.Text.Json.Serialization;

namespace Resilient_AI_Gateway.Models;

public class HuggingFaceModelsResponse
{
    [JsonPropertyName("Object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("Data")]
    public List<HuggingFaceModel> Models { get; set; } = [];
}

public class HuggingFaceModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = "model";

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = "huggingface";
}

public class ModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; set; } = "huggingface";

    [JsonPropertyName("created")]
    public long Created { get; set; }
}
