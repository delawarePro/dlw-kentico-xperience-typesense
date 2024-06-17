using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Declares members for <see cref="TypesenseCollectionLanguageItemInfo"/> management.
/// </summary>
public partial interface ITypesenseCollectionLanguageItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
