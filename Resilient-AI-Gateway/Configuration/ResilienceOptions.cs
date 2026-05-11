namespace Resilient_AI_Gateway.Configuration;

public class ResilienceOptions
{
    public const string SectionName = "Resilience";

    public int GlobalTimeoutSeconds { get; set; } = 60;
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 200;
    public List<string> FallbackModels { get; set; } = [];
}