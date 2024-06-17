using Kentico.Xperience.Typesense.Admin;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.Typesense.QueueWorker;
public class BackgroundWorker : BackgroundService
{
    private readonly ITypesenseQueue queue;
    private readonly ILogger<BackgroundWorker> logger;

    private const int BatchSize = 10;

    public BackgroundWorker(ITypesenseQueue queue, ILogger<BackgroundWorker> logger)
    {
        this.queue = queue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var items = await queue.DequeueBatchAsync(BatchSize);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        logger.LogInformation($"Processing item: {item}");
                        // Add your processing logic here
                    }
                }
                await Task.Delay(1000, stoppingToken); // Delay to avoid busy-waiting
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while processing queue items.");
            }
        }

        logger.LogInformation("Background worker stopped.");
    }
}

