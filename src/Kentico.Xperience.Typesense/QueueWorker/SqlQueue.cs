
using System.Text.Json;

using CMS.Base;
using CMS.DataEngine;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.QueueWorker;
public class SqlQueue : ITypesenseQueue
{
    private readonly IIndexQueueItemInfoProvider indexQueueItemInfoProvider;

    public SqlQueue(IIndexQueueItemInfoProvider indexQueueItemInfoProvider) 
        => this.indexQueueItemInfoProvider = indexQueueItemInfoProvider;

    public async Task EnqueueTypesenseQueueItem(TypesenseQueueItem item)
    {
        var itemInfo = new IndexQueueItemInfo
        {
            CollectionEvent = JsonSerializer.Serialize(item.ItemToCollection),
            TaskType = (int)item.TaskType,
            CollectionName = item.CollectionName,
            EnqueuedAt = DateTime.Now
        };

        indexQueueItemInfoProvider.Set(itemInfo);
    }

    public async Task<TypesenseQueueItem?> DequeueAsync()
    {
        var result = await DequeueBatchAsync(1);
        return result?.FirstOrDefault();
    }

    public async Task<IEnumerable<TypesenseQueueItem?>?> DequeueBatchAsync(int maxItems)
    {
        var query = indexQueueItemInfoProvider.Get()
                    .OrderBy(nameof(IndexQueueItemInfo.EnqueuedAt))
                    .TopN(maxItems);

        if (query != null)
        {
            var result = await query.GetEnumerableTypedResultAsync();
            if(result != null)
            {
                return result.Select(x => x == null ? null : new TypesenseQueueItem(
                    JsonSerializer.Deserialize<ICollectionEventItemModel>(x.CollectionEvent),
                    (TypesenseTaskType)x.TaskType,
                    x.CollectionName));
            }
        }

        return null;
    }
}
