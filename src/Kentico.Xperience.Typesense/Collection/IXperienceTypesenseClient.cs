using Kentico.Xperience.Typesense.Search;
using Kentico.Xperience.Typesense.Xperience;

using Typesense;


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
    public Task<int> DeleteRecords(IEnumerable<string> itemGuids, string collectionName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the indices of the Typesense application with basic statistics.
    /// </summary>
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    public Task<ICollection<TypesenseCollectionStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken);

    /// <summary>
    /// GEt the aliases of the collections
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<ICollection<TypesenseCollectionAliasViewModel>> GetAliases(CancellationToken cancellationToken);

    /// <summary>
    /// Updates the Typesense index with the dynamic data in each object of the passed <paramref name="dataObjects"/>.
    /// </summary>
    /// <remarks>Logs an error if there are issues loading the node data.</remarks>
    /// <param name="dataObjects">The objects to upsert into Typesense.</param>
    /// <param name="collectionName">The index to upsert the data to.</param>
    /// <param name="importType">Create or update type</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    /// <exception cref="OverflowException" />
    /// <returns>The number of objects processed.</returns>
    public Task<int> UpsertRecords(List<TypesenseSearchResultModel> dataObjects, string collectionName, ImportType importType = ImportType.Create, CancellationToken cancellationToken = default);

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
    public Task Rebuild(string collectionName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the Typesense index by removing existing index data from Typesense.
    /// </summary>
    /// <param name="collectionName">The index to delete.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="OperationCanceledException" />
    /// <exception cref="ObjectDisposedException" />
    public Task DeleteCollection(string collectionName, CancellationToken cancellationToken);


    public Task SwapAliasWhenRebuildIsDone(string alias, string newCollectionRebuilded);
    public Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration);

    public Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration, Func<string, Task> rebuildAction);
    public Task<bool> TryDeleteCollection(ITypesenseConfigurationModel? configuration);

    public Task<(string activeCollectionName, string newCollectionName)> GetCollectionNames(string collectionName);
    public Task<int> SwapAliasWhenRebuildIsDone(IEnumerable<TypesenseQueueItem> endOfQueueItem, CancellationToken cancellationToken);
}
