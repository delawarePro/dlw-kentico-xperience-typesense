using CMS.Core;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kentico.Xperience.Typesense.QueueWorker;
public class TypesenseBackgroundWorker : BackgroundService
{
    private readonly ITypesenseQueue queue;
    private readonly IEventLogService logger;

    private const int BatchSize = 10;
    private const int DelayBetweenBatchesInMs = 10000; //TODO : This could be a parameter


    private readonly ITypesenseTaskProcessor typesenseTaskProcessor;

    public TypesenseBackgroundWorker(ITypesenseQueue queue, IEventLogService logger, ITypesenseTaskProcessor typesenseTaskProcessor)
    {
        this.typesenseTaskProcessor = typesenseTaskProcessor;
        this.queue = queue;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(nameof(TypesenseBackgroundWorker), "Background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var items = await queue.DequeueBatchAsync(BatchSize);
                if (items != null)
                {
                    int numberOfProcessed = typesenseTaskProcessor.ProcessTypesenseTasks(items, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
                    logger.LogInformation(nameof(TypesenseBackgroundWorker), $"{numberOfProcessed} items processed");
                }

                if (items == null || items.Count() < BatchSize) //If the batch was not complete
                {
                    await Task.Delay(DelayBetweenBatchesInMs, stoppingToken); // Delay to avoid busy-waiting
                }
            }
            catch (Exception ex)
            {
                logger.LogException(nameof(TypesenseBackgroundWorker), "Error occurred while processing queue items.", ex);
            }
        }

        logger.LogInformation(nameof(TypesenseBackgroundWorker), "Background worker stopped.");
    }
}

