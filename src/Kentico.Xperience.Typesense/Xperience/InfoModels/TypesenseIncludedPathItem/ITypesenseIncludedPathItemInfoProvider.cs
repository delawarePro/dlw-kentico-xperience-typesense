using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;

public partial interface ITypesenseIncludedPathItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
