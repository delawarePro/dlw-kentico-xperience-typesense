using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Collection;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.Typesense.Admin;

internal abstract class BaseCollectionEditPage : ModelEditPage<TypesenseConfigurationModel>
{
    protected readonly ITypesenseConfigurationKenticoStorageService StorageInKenticoService;
    protected readonly ITypesenseCollectionService CollectionService;

    protected BaseCollectionEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseConfigurationKenticoStorageService storageInKenticoService,
        ITypesenseCollectionService collectionService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        StorageInKenticoService = storageInKenticoService;
        CollectionService = collectionService;
    }

    protected async Task<CollectionModificationResult> ValidateAndProcess(TypesenseConfigurationModel configuration) 
        => await CollectionService.CreateOrEditCollection(configuration) ? CollectionModificationResult.Success : CollectionModificationResult.Failure;
}

internal enum CollectionModificationResult
{
    Success,
    Failure
}
