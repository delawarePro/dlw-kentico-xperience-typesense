using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin.UIPages;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
   parentType: typeof(QueryListingPage),
   slug: "create-query",
   uiPageType: typeof(QueryCreatePage),
   name: "Create query",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Typesense.Admin.UIPages;

[UIEvaluatePermission(SystemPermissions.CREATE)]
internal class QueryCreatePage : BaseQueryEditPage
{
    private readonly IPageLinkGenerator pageLinkGenerator;
    private TypesenseQueryModel? model;

    public QueryCreatePage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseQueryStorageService storageService,
        IPageLinkGenerator pageLinkGenerator,
        ITypesenseQueryService queryService)
        : base(formItemCollectionProvider, formDataBinder, storageService, queryService) =>
        this.pageLinkGenerator = pageLinkGenerator;

    protected override TypesenseQueryModel Model
    {
        get
        {
            model ??= new();
            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(TypesenseQueryModel model, ICollection<IFormItem> formItems)
    {
        var result = await ValidateAndProcess(model);

        if (result == QueryModificationResult.Success)
        {
            var successResponse = NavigateTo(pageLinkGenerator.GetPath<QueryListingPage>())
                .AddSuccessMessage("Query created.");

            return await Task.FromResult<ICommandResponse>(successResponse);
        }

        var errorResponse = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
            .AddErrorMessage("Could not create the query.");

        return await Task.FromResult<ICommandResponse>(errorResponse);
    }
}
