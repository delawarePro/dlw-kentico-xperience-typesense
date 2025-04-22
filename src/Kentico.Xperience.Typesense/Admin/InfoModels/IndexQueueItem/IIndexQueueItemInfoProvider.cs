using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Declares members for <see cref="TypesenseIndexLanguageItemInfo"/> management.
/// </summary>
public partial interface IIndexQueueItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
