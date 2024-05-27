using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Class providing <see cref="TypesenseIncludedPathItemInfo"/> management.
/// </summary>
[ProviderInterface(typeof(ITypesenseIncludedPathItemInfoProvider))]
public partial class TypesenseIncludedPathItemInfoProvider : AbstractInfoProvider<TypesenseIncludedPathItemInfo, TypesenseIncludedPathItemInfoProvider>, ITypesenseIncludedPathItemInfoProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypesenseIncludedPathItemInfoProvider"/> class.
    /// </summary>
    public TypesenseIncludedPathItemInfoProvider()
        : base(TypesenseIncludedPathItemInfo.TYPEINFO)
    {
    }
}
