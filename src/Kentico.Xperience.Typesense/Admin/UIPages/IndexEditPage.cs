using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;

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
                 ITypesenseConfigurationStorageService storageService)
        : base(formItemCollectionProvider, formDataBinder, storageService) { }

    protected override TypesenseConfigurationModel Model
    {
        get
        {
            model ??= StorageService.GetCollectionDataOrNull(CollectionIdentifier) ?? new();

            return model;
        }
    }

    protected override Task<ICommandResponse> ProcessFormData(TypesenseConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = ValidateAndProcess(model);

        var response = ResponseFrom(new FormSubmissionResult(
            result == CollectionModificationResult.Success
                ? FormSubmissionStatus.ValidationSuccess
                : FormSubmissionStatus.ValidationFailure));

        _ = result == CollectionModificationResult.Success
            ? response.AddSuccessMessage("Collection edited")
            : response.AddErrorMessage("Could not update index");

        return Task.FromResult<ICommandResponse>(response);
    }
}
