using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

using IFormItemCollectionProvider = Kentico.Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider;

[assembly: UIPage(
   parentType: typeof(CollectionListingPage),
   slug: "create",
   uiPageType: typeof(CollectionCreatePage),
   name: "Create index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Typesense.Admin;

[UIEvaluatePermission(SystemPermissions.CREATE)]
internal class CollectionCreatePage : BaseCollectionEditPage
{
    private readonly IPageUrlGenerator pageUrlGenerator;
    private TypesenseConfigurationModel? model = null;

    public CollectionCreatePage(
        IFormItemCollectionProvider formItemCollectionProvider,
        IFormDataBinder formDataBinder,
        ITypesenseConfigurationKenticoStorageService storageService,
        IPageUrlGenerator pageUrlGenerator,
        ITypesenseConfigurationTypesenseStorageService typesenseConfigurationTypesenseStorage)
        : base(formItemCollectionProvider, formDataBinder, storageService, typesenseConfigurationTypesenseStorage)
    {
        this.pageUrlGenerator = pageUrlGenerator;
    }

    protected override TypesenseConfigurationModel Model
    {
        get
        {
            model ??= new();

            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(TypesenseConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = await ValidateAndProcess(model);

        if (result == CollectionModificationResult.Success)
        {
            var index = TypesenseCollectionStore.Instance.GetRequiredCollection(model.CollectionName);

            var successResponse = NavigateTo(pageUrlGenerator.GenerateUrl<CollectionEditPage>(index.Identifier.ToString()))
                .AddSuccessMessage("Collection created.");

            return await Task.FromResult<ICommandResponse>(successResponse);
        }

        var errorResponse = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
            .AddErrorMessage("Could not create the collection.");

        return await Task.FromResult<ICommandResponse>(errorResponse);
    }
}
