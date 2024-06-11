using Kentico.Xperience.Typesense.Xperience;

using System.Text;


namespace Kentico.Xperience.Typesense.Collection;

public class DefaultTypesenseCollectionService : ITypesenseCollectionService
{
    private readonly ITypesenseConfigurationKenticoStorageService typesenseConfigurationKenticoStorageService;
    private readonly IXperienceTypesenseClient xperienceTypesenseClient;

    public DefaultTypesenseCollectionService(ITypesenseConfigurationKenticoStorageService typesenseConfigurationKenticoStorageService,
        IXperienceTypesenseClient xperienceTypesenseClient)
    {
        this.xperienceTypesenseClient = xperienceTypesenseClient;
        this.typesenseConfigurationKenticoStorageService = typesenseConfigurationKenticoStorageService;
    }

    public async Task<bool> CreateOrEditCollection(ITypesenseConfigurationModel configuration)
    {
        static string RemoveWhitespacesUsingStringBuilder(string source)
        {
            var builder = new StringBuilder(source.Length);
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];
                if (!char.IsWhiteSpace(c))
                {
                    builder.Append(c);
                }
            }
            return source.Length == builder.Length ? source : builder.ToString();
        }

        configuration.CollectionName = RemoveWhitespacesUsingStringBuilder(configuration.CollectionName ?? "");

        if (typesenseConfigurationKenticoStorageService.GetCollectionIds().Exists(x => x == configuration.Id))
        {
            bool edited = await typesenseConfigurationKenticoStorageService.TryEditCollection(configuration);

            if (edited)
            {
                TypesenseSearchModule.AddRegisteredCollections();

                bool sucessInTypesense = await xperienceTypesenseClient.TryEditCollection(configuration, async (colName) => await xperienceTypesenseClient.Rebuild(colName, default));

                if (sucessInTypesense)
                {
                    return true;
                }
            }

            return false;
        }
        else
        {
            bool created = !string.IsNullOrWhiteSpace(configuration.CollectionName);
            created &= await typesenseConfigurationKenticoStorageService.TryCreateCollection(configuration);

            if (created)
            {
                TypesenseCollectionStore.Instance.AddCollection(new TypesenseCollection(configuration, StrategyStorage.Strategies));

                bool sucessInTypesense = await xperienceTypesenseClient.TryCreateCollection(configuration);

                if (sucessInTypesense)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
