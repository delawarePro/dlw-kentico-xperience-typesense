namespace Kentico.Xperience.Typesense.Query;

public interface ITypesenseQueryService
{
    public Task<bool> CreateOrEditQuery(TypesenseQueryAdminModel query);
}
