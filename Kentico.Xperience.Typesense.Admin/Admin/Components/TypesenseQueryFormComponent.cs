using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Typesense.Admin.Components;
using Kentico.Xperience.Typesense.Queries;

[assembly: RegisterFormComponent(TypesenseQueryFormComponent.IDENTIFIER,
                                 typeof(TypesenseQueryFormComponent),
                                 "Typesense query selector")]
namespace Kentico.Xperience.Typesense.Admin.Components;

[ComponentAttribute(typeof(TypesenseQueryFormComponentAttribute))]
public class TypesenseQueryFormComponent(ITypesenseQueryStorageService typesenseQueryStorageService) : FormComponent<TypesenseQueryFormComponentProperties, TypesenseQueryFormComponentClientProperties, string>
{
    public const string IDENTIFIER = "delaware.xperience-integrations-typesense.query-component";

    public override string ClientComponentName => "@kentico/xperience-admin-base/DropDownSelector";


    protected override async Task ConfigureClientProperties(TypesenseQueryFormComponentClientProperties clientProperties)
    {
        await base.ConfigureClientProperties(clientProperties);

        try
        {
            var queries = typesenseQueryStorageService.GetAllQueryBasicInfo();

            if (queries is not null)
            {
                var options = queries
                        .Select(x => new DropDownOptionItem
                        {
                            Text = x.TypesenseQueryItemQueryName,
                            Value = x.TypesenseQueryItemQueryName
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
public class TypesenseQueryFormComponentProperties : FormComponentProperties
{
}
public class TypesenseQueryFormComponentClientProperties : DropDownClientProperties
{
}
public class TypesenseQueryFormComponentAttribute : FormComponentAttribute
{
}


