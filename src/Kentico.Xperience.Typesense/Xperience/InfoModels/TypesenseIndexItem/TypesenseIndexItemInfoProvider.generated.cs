using CMS.DataEngine;

using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="TypesenseIndexItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(ITypesenseCollectionItemInfoProvider))]
public partial class TypesenseCollectionItemInfoProvider : AbstractInfoProvider<TypesenseIndexItemInfo, TypesenseCollectionItemInfoProvider>, ITypesenseCollectionItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseCollectionItemInfoProvider"/> class.
    /// </summary>
    public TypesenseCollectionItemInfoProvider()
        : base(TypesenseIndexItemInfo.TYPEINFO)
    {
    }
}
