namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Contains methods for logging <see cref="TypesenseQueueItem"/>s and <see cref="TypesenseQueueItem"/>s
/// for processing by <see cref="TypesenseQueueWorker"/> and <see cref="TypesenseQueueWorker"/>.
/// </summary>
public interface ITypesenseTaskLogger
{
    /// <summary>
    /// Logs an <see cref="TypesenseQueueItem"/> for each registered crawler. Then, loops
    /// through all registered Typesense indexes and logs a task if the passed <paramref name="webpageItem"/> is indexed.
    /// </summary>
    /// <param name="webpageItem">The <see cref="CollectionEventWebPageItemModel"/> that triggered the event.</param>
    /// <param name="eventName">The name of the Xperience event that was triggered.</param>
    Task HandleEvent(CollectionEventWebPageItemModel webpageItem, string eventName);

    Task HandleReusableItemEvent(CollectionEventReusableItemModel reusableItem, string eventName);
}
