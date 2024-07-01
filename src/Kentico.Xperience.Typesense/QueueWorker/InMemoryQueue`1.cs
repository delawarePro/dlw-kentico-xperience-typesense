using System.Threading.Channels;

using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.QueueWorker;

public class InMemoryQueue : ITypesenseQueue
{
    private readonly Channel<TypesenseQueueItem> channel;

    public InMemoryQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        channel = Channel.CreateBounded<TypesenseQueueItem>(options);
    }

    public async Task EnqueueTypesenseQueueItem(TypesenseQueueItem item) => await channel.Writer.WriteAsync(item);

    public async Task<TypesenseQueueItem?> DequeueAsync() => await channel.Reader.ReadAsync();

    public async Task<IEnumerable<TypesenseQueueItem>?> DequeueBatchAsync(int maxItems)
    {
        var items = new List<TypesenseQueueItem>();
        while (items.Count < maxItems && channel.Reader.Count > 0)
        {
            items.Add(await channel.Reader.ReadAsync());
        }
        return items;
    }
}
