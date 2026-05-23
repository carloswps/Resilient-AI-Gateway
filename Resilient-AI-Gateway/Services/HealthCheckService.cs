using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resilient_AI_Gateway.Configuration;

namespace Resilient_AI_Gateway.Services;


public class GatewayHealthCheck : IHealthCheck
{

    private readonly MongoClient _mongoClient;
    private readonly string _databaseName;
    private readonly HttpClient _httpClient;
    private readonly HuggingFaceOptions _hfOptions;

    public GatewayHealthCheck(
        IOptions<MongoDbOptions> mongoOptions,
        IHttpClientFactory httpClientFactory,
        IOptions<HuggingFaceOptions> hfOptions)
    {
        _mongoClient = new MongoClient(mongoOptions.Value.ConnectionString);
        _databaseName = mongoOptions.Value.DatabaseName;
        _httpClient = httpClientFactory.CreateClient("HealthCheck");
        _hfOptions = hfOptions.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();
        var allHealthy = true;

        try
        {
            var db = _mongoClient.GetDatabase(_databaseName);
            await db.RunCommandAsync<MongoDB.Bson.BsonDocument>(
                new MongoDB.Bson.BsonDocument("ping", 1),
                cancellationToken: cancellationToken
            );

            data["mongodb"] = "Healthy";
        }
        catch (Exception ex)
        {
            data["mongodb"] = $"Unhealthy: {ex.Message}";
            allHealthy = false;
        }


        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _hfOptions.BaseUrl.TrimEnd('/') + "/chat/completions")
            {
                Content = System.Net.Http.Json.JsonContent.Create(new
                {
                    model = "Qwen/Qwen2.5-7B-Instruct",
                    messages = new[] { new { role = "user", content = "ping" } },
                    max_tokens = 1
                })
            };

            request.Headers.Add("Authorization", $"Bearer {_hfOptions.ApiToken}");

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var response = await _httpClient.SendAsync(request, linkedCts.Token);
            data["huggingface_connectivity"] = response.IsSuccessStatusCode ? "Healthy" : $"Responded with status code: {response.StatusCode}";
            if (!response.IsSuccessStatusCode) allHealthy = false;
        }
        catch (Exception ex)
        {
            data["huggingface_connectivity"] = $"Unhealthy: {ex.Message}";
            allHealthy = false;
        }

        return allHealthy
            ? HealthCheckResult.Healthy("All services are healthy.", data)
            : HealthCheckResult.Unhealthy("One or more services are unhealthy.", null, data);

    }
}
