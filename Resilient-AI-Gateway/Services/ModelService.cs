using Microsoft.Extensions.Options;
using Resilient_AI_Gateway.Configuration;
using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public class ModelService : IModelService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ModelService> _logger;

    private List<ModelInfo>? _cacheModels;
    private DateTime _cacheExpiration = DateTime.MinValue;
    private readonly TimeSpan _cacheRefreshInterval = TimeSpan.FromMinutes(5);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public ModelService(IHttpClientFactory httpClientFactory, ILogger<ModelService> logger, IOptions<HuggingFaceOptions> hfOptions)
    {
        _httpClient = httpClientFactory.CreateClient("ModelService");
        _httpClient.BaseAddress = new Uri(hfOptions.Value.BaseUrl.TrimEnd('/') + "/");
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {hfOptions.Value.ApiToken}");
        _logger = logger;
    }

    public async Task<List<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheExpiration != default && DateTime.UtcNow < _cacheExpiration)
        {
            return _cacheModels;
        }

        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            if (_cacheExpiration != default && DateTime.UtcNow < _cacheExpiration)
            {
                return _cacheModels;
            }

            _logger.LogInformation("Fetching available models from Hugging Face router API...");

            var response = await _httpClient.GetAsync("models", cancellationToken);
            response.EnsureSuccessStatusCode();

            var modelsResponse = await response.Content.ReadFromJsonAsync<HuggingFaceModelsResponse>(cancellationToken: cancellationToken);

            if (modelsResponse?.Models is null || modelsResponse.Models.Count == 0)
            {
                _logger.LogWarning("Hugging Face returned an empty model list.");
                return _cacheModels ?? [];
            }

            _cacheModels = modelsResponse.Models
                           .Select(m => new ModelInfo { Id = m.Id, OwnedBy = m.OwnedBy, Created = m.Created })
                           .OrderBy(m => m.Id)
                           .ToList();

            _cacheExpiration = DateTime.UtcNow.Add(_cacheRefreshInterval);
            _logger.LogInformation("Cached {Count} models from Hugging Face.", _cacheModels.Count);

            return _cacheModels;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch models from Hugging Face router API.");
            if (_cacheModels is not null) return _cacheModels;
            throw;
        }
        finally
        {
            _refreshLock.Release();
        }
    }
    public void Dispose() => _refreshLock.Dispose();
}
