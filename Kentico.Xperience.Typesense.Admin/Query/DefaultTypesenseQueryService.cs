namespace Kentico.Xperience.Typesense.Admin;

public class DefaultTypesenseQueryService : ITypesenseQueryService
{
    private readonly ITypesenseQueryStorageService queryStorageService;

    public DefaultTypesenseQueryService(ITypesenseQueryStorageService queryStorageService)
    {
        this.queryStorageService = queryStorageService;
    }

    public async Task<bool> CreateOrEditQuery(TypesenseQueryModel query)
    {
        if (queryStorageService.GetQueryIds().Contains(query.Id))
        {
            return await queryStorageService.TryEditQuery(query);
        }
        else
        {
            return await queryStorageService.TryCreateQuery(query);
        }
    }
}
