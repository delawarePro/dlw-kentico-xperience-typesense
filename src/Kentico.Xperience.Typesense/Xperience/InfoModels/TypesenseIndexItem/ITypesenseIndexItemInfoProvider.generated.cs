using CMS.DataEngine;

using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

/// <summary>
/// Declares members for <see cref="TypesenseCollectionItemInfo"/> management.
/// </summary>
public partial interface ITypesenseCollectionItemInfoProvider : IInfoProvider<TypesenseCollectionItemInfo>, IInfoByIdProvider<TypesenseCollectionItemInfo>, IInfoByNameProvider<TypesenseCollectionItemInfo>
{
}
