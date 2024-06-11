using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;

public partial interface ITypesenseCollectionItemInfoProvider
{
    void BulkDelete(IWhereCondition where, BulkDeleteSettings? settings = null);
}
