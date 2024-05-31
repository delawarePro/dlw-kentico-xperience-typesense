namespace Kentico.Xperience.Typesense.Collectioning;

/// <summary>
/// Initializes <see cref="ISearchCollection" /> instances.
/// </summary>
public interface ITypesenseCollectionService
{
    /// <summary>
    /// Initializes a new <see cref="ISearchCollection" /> for the given <paramref name="collectionName" />
    /// </summary>
    /// <param name="collectionName">The code name of the index.</param>
    /// <param name="cancellationToken">The cancellation token for the task.</param>
    /// <exception cref="InvalidOperationException" />
    Task InitializeCollection(string collectionName, CancellationToken cancellationToken);
}
