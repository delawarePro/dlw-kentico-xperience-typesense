using CMS.Base;
using CMS.Core;

using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense;

/// <summary>
/// Thread worker which enqueues recently updated or deleted nodes indexed
/// by Typesense and processes the tasks in the background thread.
/// </summary>
internal class TypesenseQueueWorker : ThreadQueueWorker<TypesenseQueueItem, TypesenseQueueWorker>
{
    private readonly ITypesenseTaskProcessor typesenseTaskProcessor;


    /// <inheritdoc />
    protected override int DefaultInterval => 10000;


    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseQueueWorker"/> class.
    /// Should not be called directly- the worker should be initialized during startup using
    /// <see cref="ThreadWorker{T}.EnsureRunningThread"/>.
    /// </summary>
    public TypesenseQueueWorker() => typesenseTaskProcessor = Service.Resolve<ITypesenseTaskProcessor>();


    /// <summary>
    /// Adds an <see cref="TypesenseQueueItem"/> to the worker queue to be processed.
    /// </summary>
    /// <param name="queueItem">The item to be added to the queue.</param>
    /// <exception cref="InvalidOperationException" />
    public static void EnqueueTypesenseQueueItem(TypesenseQueueItem queueItem)
    {
        if (queueItem == null || queueItem.ItemToCollection == null || string.IsNullOrEmpty(queueItem.CollectionName))
        {
            return;
        }

        if (queueItem.TaskType == TypesenseTaskType.UNKNOWN)
        {
            return;
        }

        if (TypesenseCollectionStore.Instance.GetCollection(queueItem.CollectionName) == null)
        {
            throw new InvalidOperationException($"Attempted to log task for Typesense index '{queueItem.CollectionName},' but it is not registered.");
        }

        Current.Enqueue(queueItem, false);
    }


    /// <inheritdoc />
    protected override void Finish() => RunProcess();


    /// <inheritdoc/>
    protected override void ProcessItem(TypesenseQueueItem item)
    {
    }


    /// <inheritdoc />
    protected override int ProcessItems(IEnumerable<TypesenseQueueItem> items) => typesenseTaskProcessor.ProcessTypesenseTasks(items, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
}
