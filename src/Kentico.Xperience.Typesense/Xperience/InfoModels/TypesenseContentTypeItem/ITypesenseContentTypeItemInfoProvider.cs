using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;

/// <summary>
/// Declares members for <see cref="TypesenseContentTypeItemInfo"/> management.
/// </summary>
public partial interface ITypesenseContentTypeItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
