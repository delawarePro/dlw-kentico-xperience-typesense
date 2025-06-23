using CMS.Membership;
using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Admin.UIPages;

[assembly: UIPage(
   parentType: typeof(QueryListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(QueryEditPage),
   name: "Edit query",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Typesense.Admin;

[UIEvaluatePermission(SystemPermissions.UPDATE)]
internal class QueryEditPage : BaseQueryEditPage
{
    private TypesenseQueryModel? model = null;

    [PageParameter(typeof(IntPageModelBinder))]
    public int QueryIdentifier { get; set; }

    public QueryEditPage(IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 ITypesenseQueryStorageService storageService,
                 ITypesenseQueryService queryService)
        : base(formItemCollectionProvider, formDataBinder, storageService, queryService) { }

    protected override TypesenseQueryModel Model
    {
        get
        {
            model ??= StorageService.GetQueryDataOrNull(QueryIdentifier) ?? new();
            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(TypesenseQueryModel model, ICollection<IFormItem> formItems)
    {
        var result = await ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result == QueryModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        _ = result == QueryModificationResult.Success
            ? response.AddSuccessMessage("Query edited")
            : response.AddErrorMessage("Could not update query");

        return await Task.FromResult<ICommandResponse>(response);
    }
}
