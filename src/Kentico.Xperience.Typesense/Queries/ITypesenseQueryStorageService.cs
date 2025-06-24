using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryItem;

namespace Kentico.Xperience.Typesense.Queries;

public interface ITypesenseQueryStorageService
{
    public Task<bool> TryCreateQuery(TypesenseQueryModel query);
    public Task<bool> TryEditQuery(TypesenseQueryModel query);
    public Task<bool> TryDeleteQuery(TypesenseQueryModel query);
    public Task<bool> TryDeleteQuery(int queryId);
    public TypesenseQueryModel? GetQueryDataOrNull(int queryId);
    public List<string> GetExistingCollectionAliases();
    public List<int> GetQueryIds();
    public IEnumerable<TypesenseQueryModel> GetAllQueryData();
    public TypesenseQueryModel? GetQueryDataByName(string queryName);
    public IEnumerable<TypesenseQueryItemInfo> GetAllQueryBasicInfo();
}
