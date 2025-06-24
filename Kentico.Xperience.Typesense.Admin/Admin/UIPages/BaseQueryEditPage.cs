using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Queries;
using Kentico.Xperience.Typesense.Query;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

namespace Kentico.Xperience.Typesense.Admin.UIPages;

internal abstract class BaseQueryEditPage : ModelEditPage<TypesenseQueryAdminModel>
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

    protected async Task<QueryModificationResult> ValidateAndProcess(TypesenseQueryAdminModel query)
        => await QueryService.CreateOrEditQuery(query) ? QueryModificationResult.Success : QueryModificationResult.Failure;
}

internal enum QueryModificationResult
{
    Success,
    Failure
}
