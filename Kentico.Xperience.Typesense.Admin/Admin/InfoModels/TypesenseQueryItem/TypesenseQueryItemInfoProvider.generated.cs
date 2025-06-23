using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="TypesenseQueryItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(ITypesenseQueryItemInfoProvider))]
public partial class TypesenseQueryItemInfoProvider : AbstractInfoProvider<TypesenseQueryItemInfo, TypesenseQueryItemInfoProvider>, ITypesenseQueryItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseQueryItemInfoProvider"/> class.
    /// </summary>
    public TypesenseQueryItemInfoProvider()
        : base(TypesenseQueryItemInfo.TYPEINFO)
    {
    }
}
