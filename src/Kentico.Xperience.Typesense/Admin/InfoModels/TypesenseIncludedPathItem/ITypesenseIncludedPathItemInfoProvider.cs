using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

public partial interface ITypesenseIncludedPathItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
