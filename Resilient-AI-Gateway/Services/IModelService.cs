using Resilient_AI_Gateway.Models;

namespace Resilient_AI_Gateway.Services;

public interface IModelService
{
    Task<List<ModelInfo>> GetModelsAsync(CancellationToken cancellationToken = default);
}
