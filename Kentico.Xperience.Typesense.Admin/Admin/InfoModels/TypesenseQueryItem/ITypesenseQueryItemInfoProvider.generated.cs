using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Declares members for <see cref="TypesenseQueryItemInfo"/> management.
/// </summary>
public partial interface ITypesenseQueryItemInfoProvider : IInfoProvider<TypesenseQueryItemInfo>, IInfoByIdProvider<TypesenseQueryItemInfo>, IInfoByNameProvider<TypesenseQueryItemInfo>
{
}
