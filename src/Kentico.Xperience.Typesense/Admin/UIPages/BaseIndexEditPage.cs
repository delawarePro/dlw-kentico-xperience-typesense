using System.Text;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Collection;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.Typesense.Admin;

internal abstract class BaseCollectionEditPage : ModelEditPage<TypesenseConfigurationModel>
{
    protected readonly ITypesenseConfigurationKenticoStorageService StorageInKenticoService;
    protected readonly ITypesenseConfigurationTypesenseStorageService StorageInTypesenseService;

    protected BaseCollectionEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseConfigurationKenticoStorageService storageInKenticoService,
        ITypesenseConfigurationTypesenseStorageService storageInTypesenseService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        StorageInKenticoService = storageInKenticoService;
        StorageInTypesenseService = storageInTypesenseService;
    }

    protected async Task<CollectionModificationResult> ValidateAndProcess(TypesenseConfigurationModel configuration)
    {
        configuration.CollectionName = RemoveWhitespacesUsingStringBuilder(configuration.CollectionName ?? "");

        if (StorageInKenticoService.GetCollectionIds().Exists(x => x == configuration.Id))
        {
            bool edited = await StorageInKenticoService.TryEditCollection(configuration);

            if (edited)
            {
                TypesenseSearchModule.AddRegisteredCollections();

                bool sucessInTypesense = await StorageInTypesenseService.TryEditCollection(configuration);

                if (sucessInTypesense)
                {
                    return CollectionModificationResult.Success;
                }
            }

            return CollectionModificationResult.Failure;
        }
        else
        {
            bool created = !string.IsNullOrWhiteSpace(configuration.CollectionName);
            created &= await StorageInKenticoService.TryCreateCollection(configuration);

            if (created)
            {
                TypesenseCollectionStore.Instance.AddCollection(new TypesenseCollection(configuration, StrategyStorage.Strategies));

                bool sucessInTypesense = await StorageInTypesenseService.TryCreateCollection(configuration);

                if (sucessInTypesense)
                {
                    return CollectionModificationResult.Success;
                }
            }

            return CollectionModificationResult.Failure;
        }
    }

    protected static string RemoveWhitespacesUsingStringBuilder(string source)
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
}

internal enum CollectionModificationResult
{
    Success,
    Failure
}
