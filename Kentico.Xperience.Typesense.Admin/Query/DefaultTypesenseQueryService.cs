using Kentico.Xperience.Typesense.Queries;

namespace Kentico.Xperience.Typesense.Query;

public class DefaultTypesenseQueryService(ITypesenseQueryStorageService queryStorageService) : ITypesenseQueryService
{
    public async Task<bool> CreateOrEditQuery(TypesenseQueryAdminModel query)
    {
        if (queryStorageService.GetQueryIds().Contains(query.Id))
        {
            return await queryStorageService.TryEditQuery(query.ToModel());
        }
        else
        {
            return await queryStorageService.TryCreateQuery(query.ToModel());
        }
    }


}
