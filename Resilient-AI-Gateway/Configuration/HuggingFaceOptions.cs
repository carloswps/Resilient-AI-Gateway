namespace Resilient_AI_Gateway.Configuration;

public class HuggingFaceOptions
{
    public const string SectionName = "HuggingFace";

    public string ApiToken { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://router.huggingface.co/v1";
}