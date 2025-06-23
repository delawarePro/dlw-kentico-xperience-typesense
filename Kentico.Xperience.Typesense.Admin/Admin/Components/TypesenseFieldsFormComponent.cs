using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin.Components;
using Kentico.Xperience.Typesense.Collection;

[assembly: RegisterFormComponent(TypesenseFieldsFormComponent.IDENTIFIER,
                                 typeof(TypesenseFieldsFormComponent),
                                 "Typesense field selector")]
namespace Kentico.Xperience.Typesense.Admin.Components;

[ComponentAttribute(typeof(TypesenseFieldsFormComponentAttribute))]
public class TypesenseFieldsFormComponent(IXperienceTypesenseClient xperienceTypesenseClient) : FormComponent<TypesenseFieldsFormComponentProperties, TypesenseFieldsFormComponentClientProperties, string>
{
    public const string IDENTIFIER = "delaware.xperience-integrations-typesense.field-component";

    public override string ClientComponentName => "@kentico/xperience-admin-base/DropDownSelector";


    protected override async Task ConfigureClientProperties(TypesenseFieldsFormComponentClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        string alias = Properties.TypesenseAlias;

        if (string.IsNullOrEmpty(alias))
        {
            clientProperties.Options = [new DropDownOptionItem { Text = "The collection alias is not set in the page type field", Value = "" }];
            return;
        }

        try
        {
            var collection = await xperienceTypesenseClient.GetCollectionDetails(alias);

            if (collection is not null)
            {
                var options = collection.Fields
                    .Where(field => !string.IsNullOrEmpty(field?.Name))
                    .Select(field => new DropDownOptionItem
                    {
                        Text = field.Name,
                        Value = field.Name
                    });
                clientProperties.Options = options;
            }
        }
        catch (Exception)
        {
            clientProperties.Options = [new DropDownOptionItem { Text = "Failed to load enum values", Value = "" }];
        }
    }
}
public class TypesenseFieldsFormComponentProperties : FormComponentProperties
{
    [DropDownComponent(
        Label = "Typesense collection alias",
        ExplanationText = "Select the typesense collection",
        DataProviderType = typeof(TypesenseCollectionAliasesOptionProvider),
        Order = 50)]
    public string TypesenseAlias { get; set; } = "";
}
public class TypesenseFieldsFormComponentClientProperties : DropDownClientProperties
{
}
public class TypesenseFieldsFormComponentAttribute : FormComponentAttribute
{
}


