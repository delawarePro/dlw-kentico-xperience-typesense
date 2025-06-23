namespace Kentico.Xperience.Typesense.Admin;

public interface ITypesenseQueryService
{
    Task<bool> CreateOrEditQuery(TypesenseQueryModel query);
}
