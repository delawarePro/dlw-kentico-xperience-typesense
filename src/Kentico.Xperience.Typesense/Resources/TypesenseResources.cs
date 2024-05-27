using CMS.Base;
using CMS.Localization;

using Kentico.Xperience.Typesense.Resources;

[assembly: RegisterLocalizationResource(typeof(TypesenseResources), SystemContext.SYSTEM_CULTURE_NAME)]
namespace Kentico.Xperience.Typesense.Resources;

internal class TypesenseResources
{
    public TypesenseResources()
    {
    }
}
