using CMS.DataEngine;

using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

/// <summary>
/// Declares members for <see cref="TypesenseIndexItemInfo"/> management.
/// </summary>
public partial interface ITypesenseCollectionItemInfoProvider : IInfoProvider<TypesenseIndexItemInfo>, IInfoByIdProvider<TypesenseIndexItemInfo>, IInfoByNameProvider<TypesenseIndexItemInfo>
{
}
