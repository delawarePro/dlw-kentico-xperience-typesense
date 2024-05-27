using Microsoft.Extensions.DependencyInjection;

using Typesense;

namespace Kentico.Xperience.Typesense.Collectioning;

/// <summary>
/// Default implementation of <see cref="ITypesenseCollectionService"/>.
/// </summary>
internal class DefaultTypesenseCollectionService : ITypesenseCollectionService
{
    private readonly ITypesenseClient searchClient;
    private readonly IServiceProvider serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTypesenseCollectionService"/> class.
    /// </summary>
    public DefaultTypesenseCollectionService(
        ITypesenseClient searchClient,
        IServiceProvider serviceProvider)
    {
        this.searchClient = searchClient;
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public async Task<CollectionResponse> InitializeCollection(string collectionName, CancellationToken cancellationToken)
    {
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(collectionName) ?? throw new InvalidOperationException($"Registered index with name '{collectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        return await searchClient.CreateCollection(indexSettings.ToSchema(collectionName));
    }
}
