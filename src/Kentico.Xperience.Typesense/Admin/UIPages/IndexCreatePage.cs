﻿using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;
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
        ITypesenseConfigurationStorageService storageService,
        IPageUrlGenerator pageUrlGenerator)
        : base(formItemCollectionProvider, formDataBinder, storageService) => this.pageUrlGenerator = pageUrlGenerator;

    protected override TypesenseConfigurationModel Model
    {
        get
        {
            model ??= new();

            return model;
        }
    }

    protected override Task<ICommandResponse> ProcessFormData(TypesenseConfigurationModel model, ICollection<IFormItem> formItems)
    {
        var result = ValidateAndProcess(model);

        if (result == CollectionModificationResult.Success)
        {
            var index = TypesenseCollectionStore.Instance.GetRequiredCollection(model.CollectionName);

            var successResponse = NavigateTo(pageUrlGenerator.GenerateUrl<CollectionEditPage>(index.Identifier.ToString()))
                .AddSuccessMessage("Collection created.");

            return Task.FromResult<ICommandResponse>(successResponse);
        }

        var errorResponse = ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
            .AddErrorMessage("Could not create index.");

        return Task.FromResult<ICommandResponse>(errorResponse);
    }
}