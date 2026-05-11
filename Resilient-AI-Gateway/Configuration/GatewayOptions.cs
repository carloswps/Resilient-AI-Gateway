namespace Resilient_AI_Gateway.Configuration;

public class GatewayOptions
{
    public const string SectionName = "Gateway";

    public string[] ApiKeys { get; set; } = [];
}