using Kentico.Xperience.Typesense.Search;
using Kentico.Xperience.Typesense.Xperience;


namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Contains methods to interface with the Typesense API.
/// </summary>
public interface IXperienceTypesenseClient
{
    /// <summary>
    /// Removes records from the 
    /// index.
    /// </summary>
    /// <param name="itemGuids">The Typesense internal IDs of the records to delete.</param>
    /// <param name="collectionName">The index containing the objects to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// 
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of records deleted.</returns>
    Task<int> DeleteRecords(IEnumerable<string> itemGuids, string collectionName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the indices of the Typesense application with basic statistics.
    /// </summary>
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task<ICollection<TypesenseCollectionStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the Typesense index with the dynamic data in each object of the passed <paramref name="dataObjects"/>.
    /// </summary>
    /// <remarks>Logs an error if there are issues loading the node data.</remarks>
    /// <param name="dataObjects">The objects to upsert into Typesense.</param>
    /// <param name="collectionName">The index to upsert the data to.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of objects processed.</returns>
    Task<int> UpsertRecords(IEnumerable<TypesenseSearchResultModel> dataObjects, string collectionName, CancellationToken cancellationToken);

    /// <summary>
    /// Rebuilds the Typesense index by removing existing data from Typesense and indexing all
    /// pages in the content tree included in the index.
    /// </summary>
    /// <param name="collectionName">The index to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task Rebuild(string collectionName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the Typesense index by removing existing index data from Typesense.
    /// </summary>
    /// <param name="collectionName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    Task DeleteCollection(string collectionName, CancellationToken cancellationToken);


    Task SwapAliasWhenRebuildIsDone(string alias, string newCollectionRebuilded);
    Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration);

    Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration, Func<string, Task> rebuildAction);
    Task<bool> TryDeleteCollection(ITypesenseConfigurationModel? configuration);

    Task<(string activeCollectionName, string newCollectionName)> GetCollectionNames(string collectionName);
    Task<int> SwapAliasWhenRebuildIsDone(IEnumerable<TypesenseQueueItem> endOfQueueItem, string key, CancellationToken cancellationToken);
}
