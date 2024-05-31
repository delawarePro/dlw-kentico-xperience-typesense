using CMS.Core;
using CMS.Websites;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Default implementation of <see cref="ITypesenseTaskLogger"/>.
/// </summary>
internal class DefaultTypesenseTaskLogger : ITypesenseTaskLogger
{
    private readonly IEventLogService eventLogService;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTypesenseTaskLogger"/> class.
    /// </summary>
    public DefaultTypesenseTaskLogger(IEventLogService eventLogService, IServiceProvider serviceProvider)
    {
        this.eventLogService = eventLogService;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task HandleEvent(CollectionEventWebPageItemModel webpageItem, string eventName)
    {
        var taskType = GetTaskType(eventName);

        foreach (var typesenseCollection in TypesenseCollectionStore.Instance.GetAllIndices())
        {
            if (!webpageItem.IsCollectionedByCollection(eventLogService, typesenseCollection.CollectionName, eventName))
            {
                continue;
            }

            var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);

            if (typesenseCollection is not null)
            {
                var toReindex = await typesenseStrategy.FindItemsToReindex(webpageItem);

                if (toReindex is not null)
                {
                    foreach (var item in toReindex)
                    {
                        if (item.ItemGuid == webpageItem.ItemGuid)
                        {
                            if (taskType == TypesenseTaskType.DELETE)
                            {
                                LogCollectionTask(new TypesenseQueueItem(item, TypesenseTaskType.DELETE, typesenseCollection.CollectionName));
                            }
                            else
                            {
                                LogCollectionTask(new TypesenseQueueItem(item, TypesenseTaskType.UPDATE, typesenseCollection.CollectionName));
                            }
                        }
                    }
                }
            }
        }
    }

    public async Task HandleReusableItemEvent(CollectionEventReusableItemModel reusableItem, string eventName)
    {
        foreach (var typesenseCollection in TypesenseCollectionStore.Instance.GetAllIndices())
        {
            if (!reusableItem.IsCollectionedByCollection(eventLogService, typesenseCollection.CollectionName, eventName))
            {
                continue;
            }

            var strategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
            var toReindex = await strategy.FindItemsToReindex(reusableItem);

            if (toReindex is not null)
            {
                foreach (var item in toReindex)
                {
                    LogCollectionTask(new TypesenseQueueItem(item, TypesenseTaskType.UPDATE, typesenseCollection.CollectionName));
                }
            }
        }
    }

    /// <summary>
    /// Logs a single <see cref="TypesenseQueueItem"/>.
    /// </summary>
    /// <param name="task">The task to log.</param>
    private void LogCollectionTask(TypesenseQueueItem task)
    {
        try
        {
            TypesenseQueueWorker.EnqueueTypesenseQueueItem(task);
        }
        catch (InvalidOperationException ex)
        {
            eventLogService.LogException(nameof(DefaultTypesenseTaskLogger), nameof(LogCollectionTask), ex);
        }
    }


    private static TypesenseTaskType GetTaskType(string eventName)
    {
        if (eventName.Equals(WebPageEvents.Publish.Name, StringComparison.OrdinalIgnoreCase))
        {
            return TypesenseTaskType.UPDATE;
        }

        if (eventName.Equals(WebPageEvents.Delete.Name, StringComparison.OrdinalIgnoreCase) ||
            eventName.Equals(WebPageEvents.Archive.Name, StringComparison.OrdinalIgnoreCase))
        {
            return TypesenseTaskType.DELETE;
        }

        return TypesenseTaskType.UNKNOWN;
    }
}
