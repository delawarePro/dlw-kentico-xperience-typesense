using System.Text;

using CMS.DataEngine;

using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryFieldWeightItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryItem;

namespace Kentico.Xperience.Typesense.Admin;

internal class DefaultTypesenseQueryStorageService : ITypesenseQueryStorageService
{
    private readonly IInfoProvider<TypesenseQueryItemInfo> queryProvider;
    private readonly IInfoProvider<TypesenseQueryFieldWeightItemInfo> fieldWeightProvider;

    public DefaultTypesenseQueryStorageService(
        IInfoProvider<TypesenseQueryItemInfo> queryProvider,
        IInfoProvider<TypesenseQueryFieldWeightItemInfo> fieldWeightProvider)
    {
        this.queryProvider = queryProvider;
        this.fieldWeightProvider = fieldWeightProvider;
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

    public Task<bool> TryCreateQuery(TypesenseQueryModel query)
    {
        var existingQuery = queryProvider.Get()
            .WhereEquals(nameof(TypesenseQueryItemInfo.TypesenseQueryItemCollectionAlias), query.CollectionAlias)
            .TopN(1)
            .FirstOrDefault();

        if (existingQuery is not null)
        {
            return Task.FromResult(false);
        }

        var newInfo = new TypesenseQueryItemInfo()
        {
            TypesenseQueryItemCollectionAlias = query.CollectionAlias ?? ""
        };

        queryProvider.Set(newInfo);
        query.Id = newInfo.TypesenseQueryItemId;

        // Save field weights
        SaveFieldWeights(query.Id, query.FieldWeights);

        return Task.FromResult(true);
    }

    public TypesenseQueryModel? GetQueryDataOrNull(int queryId)
    {
        var queryInfo = queryProvider.Get().WithID(queryId).FirstOrDefault();
        if (queryInfo == default)
        {
            return default;
        }

        var fieldWeights = LoadFieldWeights(queryId);

        return new TypesenseQueryModel
        {
            Id = queryInfo.TypesenseQueryItemId,
            CollectionAlias = queryInfo.TypesenseQueryItemCollectionAlias,
            FieldWeights = fieldWeights
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

        return queryInfos.Select(query =>
        {
            var fieldWeights = LoadFieldWeights(query.TypesenseQueryItemId);
            return new TypesenseQueryModel
            {
                Id = query.TypesenseQueryItemId,
                CollectionAlias = query.TypesenseQueryItemCollectionAlias,
                FieldWeights = fieldWeights
            };
        });
    }

    public Task<bool> TryEditQuery(TypesenseQueryModel query)
    {
        query.CollectionAlias = RemoveWhitespacesUsingStringBuilder(query.CollectionAlias ?? "");

        var queryInfo = queryProvider.Get()
            .WhereEquals(nameof(TypesenseQueryItemInfo.TypesenseQueryItemId), query.Id)
            .TopN(1)
            .FirstOrDefault();

        if (queryInfo is null)
        {
            return Task.FromResult(false);
        }

        queryInfo.TypesenseQueryItemCollectionAlias = query.CollectionAlias ?? "";
        queryProvider.Set(queryInfo);

        // Update field weights
        SaveFieldWeights(query.Id, query.FieldWeights);

        return Task.FromResult(true);
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

    public Task<bool> TryDeleteQuery(TypesenseQueryModel query)
    {
        // Delete field weights first
        fieldWeightProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseQueryFieldWeightItemInfo.TypesenseQueryFieldWeightItemQueryItemId)} = {query.Id}"));

        // Delete the query
        queryProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseQueryItemInfo.TypesenseQueryItemId)} = {query.Id}"));
        return Task.FromResult(true);
    }

    private void SaveFieldWeights(int queryId, IEnumerable<TypesenseQueryFieldWeight> fieldWeights)
    {
        // Delete existing field weights
        fieldWeightProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseQueryFieldWeightItemInfo.TypesenseQueryFieldWeightItemQueryItemId)} = {queryId}"));

        // Save new field weights
        foreach (var fieldWeight in fieldWeights ?? Enumerable.Empty<TypesenseQueryFieldWeight>())
        {
            var fieldWeightInfo = new TypesenseQueryFieldWeightItemInfo()
            {
                TypesenseQueryFieldWeightItemFieldName = fieldWeight.FieldName,
                TypesenseQueryFieldWeightItemWeight = fieldWeight.Weight,
                TypesenseQueryFieldWeightItemQueryItemId = queryId,
                TypesenseQueryFieldWeightItemGuid = Guid.NewGuid()
            };

            fieldWeightProvider.Set(fieldWeightInfo);
        }
    }

    private IEnumerable<TypesenseQueryFieldWeight> LoadFieldWeights(int queryId)
    {
        var fieldWeightInfos = fieldWeightProvider.Get()
            .WhereEquals(nameof(TypesenseQueryFieldWeightItemInfo.TypesenseQueryFieldWeightItemQueryItemId), queryId)
            .GetEnumerableTypedResult();

        return fieldWeightInfos.Select(info => new TypesenseQueryFieldWeight(info));
    }
}
