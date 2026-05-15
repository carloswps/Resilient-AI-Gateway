using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resilient_AI_Gateway.Configuration;

namespace Resilient_AI_Gateway.Logging;

public class MongoRequestLogger : BackgroundService
{
    private const int MaxBatchSize = 100;
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(1);
    private readonly LoggingChannel _channel;
    private readonly IMongoCollection<RequestLogDocument> _collection;
    private readonly ILogger<MongoRequestLogger> _logger;

    public MongoRequestLogger(
        LoggingChannel channel,
        IOptions<MongoDbOptions> mongoOptions,
        ILogger<MongoRequestLogger> logger)
    {
        _channel = channel;
        _logger = logger;

        var client = new MongoClient(mongoOptions.Value.ConnectionString);
        var database = client.GetDatabase(mongoOptions.Value.DatabaseName);
        _collection = database.GetCollection<RequestLogDocument>("request_logs");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MongoRequestLogger is running.");

        var batch = new List<RequestLogDocument>();
        var reader = _channel.Reader;

        while (await reader.WaitToReadAsync(stoppingToken))
        {
            var flushTimer = Task.Delay(FlushInterval, stoppingToken);

            // Wait for the flush timer to complete or the channel to be empty
            while (batch.Count < MaxBatchSize && reader.TryRead(out var logDocument)) batch.Add(logDocument);

            if (batch.Count < MaxBatchSize)
            {
                var moreItemTask = reader.WaitToReadAsync(stoppingToken).AsTask();
                var completedTask = await Task.WhenAny(flushTimer, moreItemTask);
                if (completedTask == flushTimer && batch.Count > 0)
                {
                    await FlushBatchAsync(batch, stoppingToken);
                    batch.Clear();
                }
                else if (completedTask == moreItemTask && batch.Count > 0)
                {
                    while (batch.Count < MaxBatchSize && reader.TryRead(out var logDocument)) batch.Add(logDocument);

                    await FlushBatchAsync(batch, stoppingToken);
                    batch.Clear();
                }
                else
                {
                    await FlushBatchAsync(batch, stoppingToken);
                    batch.Clear();
                }
            }
        }
    }

    private async Task FlushBatchAsync(List<RequestLogDocument> batch, CancellationToken stoppingToken)
    {
        try
        {
            await _collection.InsertManyAsync(batch, cancellationToken: stoppingToken);
            _logger.LogDebug("Insertions {Count} documents no MongoDB", batch.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fix in insertions {Count} documents no MongoDB", batch.Count);
        }
    }
}