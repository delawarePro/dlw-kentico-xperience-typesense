using System.Threading.Channels;

using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.QueueWorker; 

public interface ITypesenseQueue
{
    Task EnqueueTypesenseQueueItem(TypesenseQueueItem item); 
    Task<TypesenseQueueItem?> DequeueAsync();
    Task<IEnumerable<TypesenseQueueItem>?> DequeueBatchAsync(int maxItems);
}
