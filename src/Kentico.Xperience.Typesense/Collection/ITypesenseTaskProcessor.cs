namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Processes tasks from <see cref="TypesenseQueueWorker"/>.
/// </summary>
public interface ITypesenseTaskProcessor
{
    /// <summary>
    /// Processes multiple queue items from all Typesense indexes in batches. Typesense
    /// automatically applies batching in multiples of 1,000 when using their API,
    /// so all queue items are forwarded to the API.
    /// </summary>
    /// <param name="queueItems">The items to process.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <returns>The number of items processed.</returns>
    Task<int> ProcessTypesenseTasks(IEnumerable<TypesenseQueueItem> queueItems, CancellationToken cancellationToken);
}
