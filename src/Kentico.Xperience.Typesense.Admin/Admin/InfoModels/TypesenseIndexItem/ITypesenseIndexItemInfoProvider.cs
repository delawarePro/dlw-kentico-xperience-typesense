using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

public partial interface ITypesenseCollectionItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
