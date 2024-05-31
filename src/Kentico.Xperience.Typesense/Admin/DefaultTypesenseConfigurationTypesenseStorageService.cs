using Kentico.Xperience.Typesense.Collection;

using Microsoft.Extensions.DependencyInjection;

using Typesense;

namespace Kentico.Xperience.Typesense.Admin;

public class DefaultTypesenseConfigurationTypesenseStorageService : ITypesenseConfigurationTypesenseStorageService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ITypesenseClient searchClient;

    public DefaultTypesenseConfigurationTypesenseStorageService(IServiceProvider serviceProvider, ITypesenseClient searchClient)
    {
        this.serviceProvider = serviceProvider;
        this.searchClient = searchClient;
    }
    public async Task<bool> TryCreateCollection(TypesenseConfigurationModel configuration)
    {
        if (configuration is null)
        {
            return false;
        }

        //Create the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(configuration.CollectionName) ?? throw new InvalidOperationException($"Registered index with name '{configuration.CollectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        return await searchClient.CreateCollection(indexSettings.ToSchema(configuration.CollectionName)) != null;
    }

    public async Task<bool> TryDeleteCollection(TypesenseConfigurationModel? configuration)
    {
        if (configuration is not null)
        {
            return await searchClient.DeleteCollection(configuration.CollectionName) != null;
        }
        return false;

    }

    public async Task<bool> TryEditCollection(TypesenseConfigurationModel configuration)
    {
        if (configuration is null)
        {
            return false;
        }
        //Update the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(configuration.CollectionName) ?? throw new InvalidOperationException($"Registered index with name '{configuration.CollectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        return await searchClient.UpdateCollection(configuration.CollectionName, indexSettings.ToUpdateSchema(configuration.CollectionName)) != null;
    }
}
