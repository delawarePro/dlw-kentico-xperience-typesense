using CMS.DataEngine;

using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

/// <summary>
/// Declares members for <see cref="TypesenseIndexLanguageItemInfo"/> management.
/// </summary>
public partial interface ITypesenseCollectionLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
