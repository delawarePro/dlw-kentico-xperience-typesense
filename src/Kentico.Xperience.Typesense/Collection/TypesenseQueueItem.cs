namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// A queued item to be processed by <see cref="TypesenseQueueWorker"/> which
/// represents a recent change made to an indexed <see cref="ItemToCollection"/> which is a representation of a WebPageItem.
/// </summary>
public class TypesenseQueueItem
{
    /// <summary>
    /// The <see cref="ItemToCollection"/> that was changed.
    /// </summary>
    public ICollectionEventItemModel ItemToCollection
    {
        get;
    }

    /// <summary>
    /// The type of the Typesense task.
    /// </summary>
    public TypesenseTaskType TaskType
    {
        get;
    }


    /// <summary>
    /// The code name of the Typesense index to be updated.
    /// </summary>
    public string CollectionName
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseQueueItem"/> class.
    /// </summary>
    /// <param name="itemToCollection">The <see cref="ICollectionEventItemModel"/> that was changed.</param>
    /// <param name="taskType">The type of the Typesense task.</param>
    /// <param name="collectionName">The code name of the Typesense index to be updated.</param>
    /// <exception cref="ArgumentNullException" />
    public TypesenseQueueItem(ICollectionEventItemModel itemToCollection, TypesenseTaskType taskType, string collectionName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        ItemToCollection = itemToCollection;
        if (taskType != TypesenseTaskType.PUBLISH_INDEX && itemToCollection == null)
        {
            throw new ArgumentNullException(nameof(itemToCollection));
        }
        TaskType = taskType;
        CollectionName = collectionName;
    }
}
