﻿using System.Text;

using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;

using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;
using Kentico.Xperience.Typesense.Xperience;
using System.Linq;

namespace Kentico.Xperience.Typesense.Admin.Admin;

internal class DefaultTypesenseConfigurationKenticoStorageService : ITypesenseConfigurationKenticoStorageService
{
    private readonly ITypesenseCollectionItemInfoProvider indexProvider;
    private readonly ITypesenseIncludedPathItemInfoProvider pathProvider;
    private readonly ITypesenseContentTypeItemInfoProvider contentTypeProvider;
    private readonly ITypesenseCollectionLanguageItemInfoProvider languageProvider;

    public DefaultTypesenseConfigurationKenticoStorageService(
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
    public async Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration)
    {
        var existingCollection = indexProvider.Get()
            .WhereEquals(nameof(TypesenseIndexItemInfo.TypesenseCollectionItemcollectionName), configuration.CollectionName)
            .TopN(1)
            .FirstOrDefault();

        if (existingCollection is not null)
        {
            return false;
        }

        //Create in Kentico
        var newInfo = new TypesenseIndexItemInfo()
        {
            TypesenseCollectionItemcollectionName = configuration.CollectionName ?? "",
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
                var languageInfo = new TypesenseIndexLanguageItemInfo()
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
    public ITypesenseConfigurationModel? GetCollectionDataOrNull(int indexId)
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
        .GetEnumerableTypedResult()
        .ToList();

        var contentTypes = DataClassInfoProvider.ProviderObject
            .Get()
            .WhereIn(
                nameof(DataClassInfo.ClassName),
                contentTypesInfoItems
                    .Select(x => x.TypesenseContentTypeItemContentTypeName)
                    .ToArray()
            ).GetEnumerableTypedResult()
            .Select(x => new TypesenseCollectionContentType(x.ClassName, x.ClassDisplayName));


        var languages = languageProvider.Get().WhereEquals(nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId), indexInfo.TypesenseCollectionItemId).GetEnumerableTypedResult();

        return (ITypesenseConfigurationModel)new TypesenseConfigurationModel(indexInfo, languages, paths, contentTypes, contentTypesInfoItems);
    }
    public List<string> GetExistingcollectionNames() => indexProvider.Get().Select(x => x.TypesenseCollectionItemcollectionName).ToList();
    public List<int> GetCollectionIds() => indexProvider.Get().Select(x => x.TypesenseCollectionItemId).ToList();
    public IEnumerable<ITypesenseConfigurationModel> GetAllCollectionData()
    {
        var indexInfos = indexProvider.Get().GetEnumerableTypedResult().ToList();
        if (indexInfos.Count == 0)
        {
            return new List<ITypesenseConfigurationModel>();
        }

        var paths = pathProvider.Get().ToList();

        var contentTypesInfoItems = contentTypeProvider
            .Get()
            .GetEnumerableTypedResult()
            .ToList();

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

        return indexInfos.Select(index => (ITypesenseConfigurationModel)new TypesenseConfigurationModel(index, languages, paths, contentTypes, contentTypesInfoItems));
    }
    public async Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration)
    {
        configuration.CollectionName = RemoveWhitespacesUsingStringBuilder(configuration.CollectionName ?? "");

        var indexInfo = indexProvider.Get()
            .WhereEquals(nameof(TypesenseIndexItemInfo.TypesenseCollectionItemId), configuration.Id)
            .TopN(1)
            .FirstOrDefault();

        if (indexInfo is null)
        {
            return false;
        }

        pathProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId)} = {configuration.Id}"));

        indexInfo.TypesenseCollectionItemRebuildHook = configuration.RebuildHook ?? "";
        indexInfo.TypesenseCollectionItemStrategyName = configuration.StrategyName ?? "";
        indexInfo.TypesenseCollectionItemChannelName = configuration.ChannelName ?? "";
        indexInfo.TypesenseCollectionItemcollectionName = configuration.CollectionName ?? "";

        indexProvider.Set(indexInfo);

        if (configuration.LanguageNames is not null)
        {
            foreach (string? language in configuration.LanguageNames)
            {
                var languageInfo = new TypesenseIndexLanguageItemInfo()
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

    public async Task<bool> TryDeleteCollection(int collectionId)
    {
        var collectionData = GetCollectionDataOrNull(collectionId);
        if (collectionData is null)
        {
            return false;
        }
        return await TryDeleteCollection(collectionData);
    }

    public async Task<bool> TryDeleteCollection(ITypesenseConfigurationModel configuration)
    {
        indexProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIndexItemInfo.TypesenseCollectionItemId)} = {configuration.Id}"));
        pathProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIncludedPathItemInfo.TypesenseIncludedPathItemCollectionItemId)} = {configuration.Id}"));
        languageProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseIndexLanguageItemInfo.TypesenseCollectionLanguageItemCollectionItemId)} = {configuration.Id}"));
        contentTypeProvider.BulkDelete(new WhereCondition($"{nameof(TypesenseContentTypeItemInfo.TypesenseContentTypeItemCollectionItemId)} = {configuration.Id}"));

        return true;
    }
}
