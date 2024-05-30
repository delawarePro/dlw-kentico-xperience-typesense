using System.Text;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Collectioning;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.Typesense.Admin;

internal abstract class BaseCollectionEditPage : ModelEditPage<TypesenseConfigurationModel>
{
    protected readonly ITypesenseConfigurationStorageService StorageService;

    protected BaseCollectionEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder) => StorageService = storageService;

    protected CollectionModificationResult ValidateAndProcess(TypesenseConfigurationModel configuration)
    {
        configuration.CollectionName = RemoveWhitespacesUsingStringBuilder(configuration.CollectionName ?? "");

        if (StorageService.GetCollectionIds().Exists(x => x == configuration.Id))
        {
            bool edited = StorageService.TryEditCollection(configuration);

            if (edited)
            {
                TypesenseSearchModule.AddRegisteredIndices();

                return CollectionModificationResult.Success;
            }

            return CollectionModificationResult.Failure;
        }
        else
        {
            bool created = !string.IsNullOrWhiteSpace(configuration.CollectionName) && StorageService.TryCreateCollection(configuration);

            if (created)
            {
                TypesenseCollectionStore.Instance.AddCollection(new TypesenseCollection(configuration, StrategyStorage.Strategies));

                return CollectionModificationResult.Success;
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
