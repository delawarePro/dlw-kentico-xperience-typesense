using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience;

[assembly: UIApplication(
    identifier: TypesenseApplicationPage.IDENTIFIER,
    type: typeof(TypesenseApplicationPage),
    slug: "typesense",
    name: "Typesense Search",
    category: BaseApplicationCategories.DEVELOPMENT,
    icon: Icons.Magnifier,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// The root application page for the Typesense integration.
/// </summary>
[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.UPDATE)]
[UIPermission(SystemPermissions.DELETE)]
[UIPermission(TypesenseCollectionPermissions.REBUILD, "Rebuild")]
internal class TypesenseApplicationPage : ApplicationPage
{
    public const string IDENTIFIER = "Delaware.Xperience.Integrations.Typesense";
}
