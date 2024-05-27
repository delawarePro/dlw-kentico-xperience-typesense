using System.Text;
using CMS.DataEngine;

namespace Kentico.Xperience.Typesense.Admin;

internal class DefaultTypesenseConfigurationStorageService : ITypesenseConfigurationStorageService
{
    private readonly ITypesenseCollectionItemInfoProvider indexProvider;
    private readonly ITypesenseIncludedPathItemInfoProvider pathProvider;
    private readonly ITypesenseContentTypeItemInfoProvider contentTypeProvider;
    private readonly ITypesenseCollectionLanguageItemInfoProvider languageProvider;

    public DefaultTypesenseConfigurationStorageService(
        ITypesenseCollectionItemInfoProvider indexProvider,
        ITypesenseIncludedPathItemInfoProvider pathProvider,
        ITypesenseContentTypeItemInfoProvider contentTypeProvider,
        ITypesenseCollectionLanguageItemInfoProvider languageProvider
    )
    {
        this.indexProvider = indexProvider;
        this.pathProvider = pathProvider;
        this.contentTypeProvider = contentTypeProvider;
        this.languageProvider = languageProvider;
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
    public bool TryCreateCollection(TypesenseConfigurationModel configuration)
    {
        var existingCollection = indexProvider.Get()
            .WhereEquals(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemcollectionName), configuration.collectionName)
            .TopN(1)
            .FirstOrDefault();

        if (existingCollection is not null)
        {
            return false;
        }

        var newInfo = new TypesenseCollectionItemInfo()
        {
            TypesenseCollectionItemcollectionName = configuration.collectionName ?? "",
            TypesenseCollectionItemChannelName = configuration.ChannelName ?? "",
            TypesenseCollectionItemStrategyName = configuration.StrategyName ?? "",
            TypesenseCollectionItemRebuildHook = configuration.RebuildHook ?? ""
        };

        indexProvider.Set(newInfo);

        configuration.Id = newInfo.TypesenseCollectionItemId;

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new TypesenseCollectionLanguageItemInfo()
                {
                    TypesenseCollectionLanguageItemName = language,
                    TypesenseCollectionLanguageItemCollectionItemId = newInfo.TypesenseCollectionItemId
                };

                languageInfo.Insert();
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new TypesenseIncludedPathItemInfo()
                {
                    TypesenseIncludedPathItemAliasPath = path.AliasPath,
                    TypesenseIncludedPathItemCollectionItemId = newInfo.TypesenseCollectionItemId
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes is not null)
                {
                    foreach (var contentType in path.ContentTypes)
                    {
                        var contentInfo = new TypesenseContentTypeItemInfo()
                        {
                            TypesenseContentTypeItemContentTypeName = contentType.ContentTypeName,
                            TypesenseContentTypeItemIncludedPathItemId = pathInfo.TypesenseIncludedPathItemId,
                            TypesenseContentTypeItemCollectionItemId = newInfo.TypesenseCollectionItemId
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }
    public TypesenseConfigurationModel? GetCollectionDataOrNull(int indexId)
    {
        var indexInfo = indexProvider.Get().WithID(indexId).FirstOrDefault();
        if (indexInfo == default)
        {
            return default;
        }

        var paths = pathProvider.Get().WhereEquals(nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId), indexInfo.TypesenseCollectionItemId).GetEnumerableTypedResult();

        var contentTypesInfoItems = contentTypeProvider
        .Get()
        .WhereEquals(nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId), indexInfo.TypesenseCollectionItemId)
        .GetEnumerableTypedResult();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.TypesenseContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new TypesenseCollectionContentType(x.ClassName, x.ClassDisplayName));


        var languages = languageProvider.Get().WhereEquals(nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId), indexInfo.TypesenseCollectionItemId).GetEnumerableTypedResult();

        return new TypesenseConfigurationModel(indexInfo, languages, paths, contentTypes);
    }
    public List<string> GetExistingcollectionNames() => indexProvider.Get().Select(x => x.TypesenseCollectionItemcollectionName).ToList();
    public List<int> GetCollectionIds() => indexProvider.Get().Select(x => x.TypesenseCollectionItemId).ToList();
    public IEnumerable<TypesenseConfigurationModel> GetAllCollectionData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return new List<TypesenseConfigurationModel>();
        }

        var paths = pathProvider.Get().ToList();

        var contentTypesInfoItems = contentTypeProvider
            .Get()
            .GetEnumerableTypedResult();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.TypesenseContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new TypesenseCollectionContentType(x.ClassName, x.ClassDisplayName));

        var languages = languageProvider.Get().ToList();

        return indexInfos.Select(index => new TypesenseConfigurationModel(index, languages, paths, contentTypes));
    }
    public bool TryEditCollection(TypesenseConfigurationModel configuration)
    {
        configuration.collectionName = RemoveWhitespacesUsingStringBuilder(configuration.collectionName ?? "");

        var indexInfo = indexProvider.Get()
            .WhereEquals(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        if (indexInfo is null)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId)} = {configuration.Id}"));

        indexInfo.TypesenseCollectionItemRebuildHook = configuration.RebuildHook ?? "";
        indexInfo.TypesenseCollectionItemStrategyName = configuration.StrategyName ?? "";
        indexInfo.TypesenseCollectionItemChannelName = configuration.ChannelName ?? "";
        indexInfo.TypesenseCollectionItemcollectionName = configuration.collectionName ?? "";

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new TypesenseCollectionLanguageItemInfo()
                {
                    TypesenseCollectionLanguageItemName = language,
                    TypesenseCollectionLanguageItemCollectionItemId = indexInfo.TypesenseCollectionItemId,
                };

                languageProvider.Set(languageInfo);
            }
        }

        if (configuration.Paths is not null)
        {
            foreach (var path in configuration.Paths)
            {
                var pathInfo = new TypesenseIncludedPathItemInfo()
                {
                    TypesenseIncludedPathItemAliasPath = path.AliasPath,
                    TypesenseIncludedPathItemCollectionItemId = indexInfo.TypesenseCollectionItemId,
                };
                pathProvider.Set(pathInfo);

                if (path.ContentTypes != null)
                {
                    foreach (var contentType in path.ContentTypes)
                    {
                        var contentInfo = new TypesenseContentTypeItemInfo()
                        {
                            TypesenseContentTypeItemContentTypeName = contentType.ContentTypeName ?? "",
                            TypesenseContentTypeItemIncludedPathItemId = pathInfo.TypesenseIncludedPathItemId,
                            TypesenseContentTypeItemCollectionItemId = indexInfo.TypesenseCollectionItemId,
                        };
                        contentInfo.Insert();
                    }
                }
            }
        }

        return true;
    }

    public bool TryDeleteCollection(int id)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId)} = {id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId)} = {id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId)} = {id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId)} = {id}"));

        return true;
    }

    public bool TryDeleteCollection(TypesenseConfigurationModel configuration)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseCollectionLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId)} = {configuration.Id}"));

        return true;
    }
}
