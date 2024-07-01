
using System.Text.Json;

using CMS.Base;
using CMS.DataEngine;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.QueueWorker;
public class SqlQueue : ITypesenseQueue
{
    private readonly IIndexQueueItemInfoProvider indexQueueItemInfoProvider;
    private readonly JsonSerializerOptions collectionEventItemModelJsonOptions;

    public SqlQueue(IIndexQueueItemInfoProvider indexQueueItemInfoProvider)
    {
        this.indexQueueItemInfoProvider = indexQueueItemInfoProvider;

        collectionEventItemModelJsonOptions = new JsonSerializerOptions
        {
            Converters = { new CollectionEventItemModelConverter() }
        };
    }

    public async Task EnqueueTypesenseQueueItem(TypesenseQueueItem item)
    {
        var itemInfo = new IndexQueueItemInfo
        {
            CollectionEvent = JsonSerializer.Serialize(item.ItemToCollection, collectionEventItemModelJsonOptions),
            TaskType = (int)item.TaskType,
            CollectionName = item.CollectionName,
            EnqueuedAt = DateTime.Now,
            IndexQueueItemGuid = Guid.NewGuid()
        };

        indexQueueItemInfoProvider.Set(itemInfo);
    }

    public async Task<TypesenseQueueItem?> DequeueAsync()
    {
        var result = await DequeueBatchAsync(1);
        return result?.FirstOrDefault();
    }

    public async Task<IEnumerable<TypesenseQueueItem>?> DequeueBatchAsync(int maxItems)
    {
        var query = indexQueueItemInfoProvider.Get()
                    .OrderBy(nameof(IndexQueueItemInfo.EnqueuedAt))
                    .TopN(maxItems);

        if (query != null)
        {
            var result = await query.GetEnumerableTypedResultAsync();
            if (result != null && result.Any())
            {
                var resultAsQueueItem = new List<TypesenseQueueItem>();
                foreach (var item in result)
                {
                    if (item != null)
                    {
                        var itemToCollection = JsonSerializer.Deserialize<ICollectionEventItemModel>(item.CollectionEvent, collectionEventItemModelJsonOptions);
                        resultAsQueueItem.Add(new TypesenseQueueItem(
                            itemToCollection,
                            (TypesenseTaskType)item.TaskType,
                            item.CollectionName));
                    }
                }

                //Delete the items from the queue
                ICollection<int> ids = result.Select(x => x.IndexQueueItemID).ToList();
                indexQueueItemInfoProvider.BulkDelete(new WhereCondition()
                    .WhereIn(nameof(IndexQueueItemInfo.IndexQueueItemID), ids));

                return resultAsQueueItem;
            }
        }

        return null;
    }
}
