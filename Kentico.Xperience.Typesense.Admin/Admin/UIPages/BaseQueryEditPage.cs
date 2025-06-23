using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.Typesense.Admin;

internal abstract class BaseQueryEditPage : ModelEditPage<TypesenseQueryModel>
{
    protected readonly ITypesenseQueryStorageService StorageService;
    protected readonly ITypesenseQueryService QueryService;

    protected BaseQueryEditPage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseQueryStorageService storageService,
        ITypesenseQueryService queryService)
        : base(formItemCollectionProvider, formDataBinder)
    {
        StorageService = storageService;
        QueryService = queryService;
    }

    protected async Task<QueryModificationResult> ValidateAndProcess(TypesenseQueryModel query)
        => await QueryService.CreateOrEditQuery(query) ? QueryModificationResult.Success : QueryModificationResult.Failure;
}

internal enum QueryModificationResult
{
    Success,
    Failure
}
