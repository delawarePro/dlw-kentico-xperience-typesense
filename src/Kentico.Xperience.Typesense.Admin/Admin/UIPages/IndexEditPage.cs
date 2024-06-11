using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

[assembly: UIPage(
   parentType: typeof(CollectionListingPage),
   slug: PageParameterConstants.PARAMETERIZED_SLUG,
   uiPageType: typeof(CollectionEditPage),
   name: "Edit index",
   templateName: TemplateNames.EDIT,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Typesense.Admin;

[UIEvaluatePermission(SystemPermissions.UPDATE)]
internal class CollectionEditPage : BaseCollectionEditPage
{
    private TypesenseConfigurationModel? model = null;

    [PageParameter(typeof(IntPageModelBinder))]
    public int CollectionIdentifier { get; set; }

    public CollectionEditPage(Xperience.Admin.Base.Forms.Internal.IFormItemCollectionProvider formItemCollectionProvider,
                 IFormDataBinder formDataBinder,
                 ITypesenseConfigurationKenticoStorageService storageService,
                 ITypesenseCollectionService collectionService)
        : base(formItemCollectionProvider, formDataBinder, storageService, collectionService) { }

    protected override TypesenseConfigurationModel Model
    {
        get
        {
            model ??= StorageInKenticoService.GetCollectionDataOrNull(CollectionIdentifier) ?? new();

            return model;
        }
    }

    protected override async Task<ICommandResponse> ProcessFormData(TypesenseConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = await ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result == CollectionModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        _ = result == CollectionModificationResult.Success
            ? response.AddSuccessMessage("Collection edited")
            : response.AddErrorMessage("Could not update index");

        return await Task.FromResult<ICommandResponse>(response);
    }
}
