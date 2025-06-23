using System.Text;

using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

internal class DefaultTypesenseQueryStorageService : ITypesenseQueryStorageService
{
    private readonly ITypesenseQueryItemInfoProvider queryProvider;

    public DefaultTypesenseQueryStorageService(ITypesenseQueryItemInfoProvider queryProvider)
    {
        this.queryProvider = queryProvider;
    }

    private static string RemoveWhitespacesUsingStringBuilder(string source)
    {
        var builder = new StringBuilder(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (!char.IsWhiteSpace(c))
            {
                builder.Append(c);
            }
        }
        return source.Length == builder.Length ? source : builder.ToString();
    }

    public async Task<bool> TryCreateQuery(TypesenseQueryModel query)
    {
        var existingQuery = queryProvider.Get()
            .WhereEquals(nameof(TypesenseQueryItemInfo.TypesenseQueryItemCollectionAlias), query.CollectionAlias)
            .TopN(1)
            .FirstOrDefault();

        if (existingQuery is not null)
        {
            return false;
        }

        var newInfo = new TypesenseQueryItemInfo()
        {
            TypesenseQueryItemCollectionAlias = query.CollectionAlias ?? ""
        };

        queryProvider.Set(newInfo);
        query.Id = newInfo.TypesenseQueryItemId;

        return true;
    }

    public TypesenseQueryModel? GetQueryDataOrNull(int queryId)
    {
        var queryInfo = queryProvider.Get().WithID(queryId).FirstOrDefault();
        if (queryInfo == default)
        {
            return default;
        }

        return new TypesenseQueryModel
        {
            Id = queryInfo.TypesenseQueryItemId,
            CollectionAlias = queryInfo.TypesenseQueryItemCollectionAlias
        };
    }

    public List<string> GetExistingCollectionAliases() => queryProvider.Get().Select(x => x.TypesenseQueryItemCollectionAlias).ToList();
    public List<int> GetQueryIds() => queryProvider.Get().Select(x => x.TypesenseQueryItemId).ToList();

    public IEnumerable<TypesenseQueryModel> GetAllQueryData()
    {
        var queryInfos = queryProvider.Get().GetEnumerableTypedResult().ToList();
        if (queryInfos.Count == 0)
        {
            return new List<TypesenseQueryModel>();
        }

        return queryInfos.Select(query => new TypesenseQueryModel
        {
            Id = query.TypesenseQueryItemId,
            CollectionAlias = query.TypesenseQueryItemCollectionAlias
        });
    }

    public async Task<bool> TryEditQuery(TypesenseQueryModel query)
    {
        query.CollectionAlias = RemoveWhitespacesUsingStringBuilder(query.CollectionAlias ?? "");

        var queryInfo = queryProvider.Get()
            .WhereEquals(nameof(TypesenseQueryItemInfo.TypesenseQueryItemId), query.Id)
            .TopN(1)
            .FirstOrDefault();

        if (queryInfo is null)
        {
            return false;
        }

        queryInfo.TypesenseQueryItemCollectionAlias = query.CollectionAlias ?? "";
        queryProvider.Set(queryInfo);

        return true;
    }

    public async Task<bool> TryDeleteQuery(int queryId)
    {
        var queryData = GetQueryDataOrNull(queryId);
        if (queryData is null)
        {
            return false;
        }
        return await TryDeleteQuery(queryData);
    }

    public async Task<bool> TryDeleteQuery(TypesenseQueryModel query)
    {
        queryProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseQueryItemInfo.TypesenseQueryItemId)} = {query.Id}"));
        return true;
    }
}
