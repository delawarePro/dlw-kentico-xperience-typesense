using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="TypesenseContentTypeItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(ITypesenseContentTypeItemInfoProvider))]
public partial class TypesenseContentTypeItemInfoProvider : AbstractInfoProvider<TypesenseContentTypeItemInfo, TypesenseContentTypeItemInfoProvider>, ITypesenseContentTypeItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseContentTypeItemInfoProvider"/> class.
    /// </summary>
    public TypesenseContentTypeItemInfoProvider()
        : base(TypesenseContentTypeItemInfo.TYPEINFO)
    {
    }
}
