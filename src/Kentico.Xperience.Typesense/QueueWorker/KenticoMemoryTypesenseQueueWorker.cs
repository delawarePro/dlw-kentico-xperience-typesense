﻿using CMS.Base;
using CMS.Core;

using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.QueueWorker;

/// <summary>
/// Thread worker which enqueues recently updated or deleted nodes indexed
/// by Typesense and processes the tasks in the background thread.
/// 
/// This is the default implementation from Kentico's algolia integration but as this isn't persistant the sql version is probably better.
/// </summary>
internal class KenticoMemoryTypesenseQueueWorker : ThreadQueueWorker<TypesenseQueueItem, KenticoMemoryTypesenseQueueWorker>
{
    private readonly ITypesenseTaskProcessor typesenseTaskProcessor;
    private readonly IXperienceTypesenseClient xperienceTypesenseClient;

    /// <inheritdoc />
    protected override int DefaultInterval => 10000;


    /// <summary>
    /// Initializes a new instance of the <see cref="KenticoMemoryTypesenseQueueWorker"/> class.
    /// Should not be called directly- the worker should be initialized during startup using
    /// <see cref="ThreadWorker{T}.EnsureRunningThread"/>.
    /// </summary>
    public KenticoMemoryTypesenseQueueWorker()
    {
        typesenseTaskProcessor = Service.Resolve<ITypesenseTaskProcessor>();
        xperienceTypesenseClient = Service.Resolve<IXperienceTypesenseClient>();
    }



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

        Current.Enqueue(queueItem, false); //TODO : Provide an abstraction for the queue to avoid thread queues
    }


    /// <inheritdoc />
    protected override void Finish() => RunProcess();


    /// <inheritdoc/>
    protected override void ProcessItem(TypesenseQueueItem item)
    {
    }


    /// <inheritdoc />
    protected override int ProcessItems(IEnumerable<TypesenseQueueItem> items)
    {
        if (items == null || !items.Any())
        {
            return 0;
        }

        int numberOfProcessed = typesenseTaskProcessor.ProcessTypesenseTasks(items, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        return numberOfProcessed;
    }
}
