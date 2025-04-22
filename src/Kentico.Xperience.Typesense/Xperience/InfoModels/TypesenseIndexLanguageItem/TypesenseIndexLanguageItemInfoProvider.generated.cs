using CMS.DataEngine;

using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="TypesenseIndexLanguageItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(ITypesenseCollectionLanguageItemInfoProvider))]
public partial class TypesenseCollectionedLanguageInfoProvider : AbstractInfoProvider<TypesenseIndexLanguageItemInfo, TypesenseCollectionedLanguageInfoProvider>, ITypesenseCollectionLanguageItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseCollectionedLanguageInfoProvider"/> class.
    /// </summary>
    public TypesenseCollectionedLanguageInfoProvider()
        : base(TypesenseIndexLanguageItemInfo.TYPEINFO)
    {
    }
}
