namespace Kentico.Xperience.Typesense.Admin;

public interface ITypesenseQueryStorageService
{
    Task<bool> TryCreateQuery(TypesenseQueryModel query);
    Task<bool> TryEditQuery(TypesenseQueryModel query);
    Task<bool> TryDeleteQuery(TypesenseQueryModel query);
    Task<bool> TryDeleteQuery(int queryId);
    TypesenseQueryModel? GetQueryDataOrNull(int queryId);
    List<string> GetExistingCollectionAliases();
    List<int> GetQueryIds();
    IEnumerable<TypesenseQueryModel> GetAllQueryData();
}
