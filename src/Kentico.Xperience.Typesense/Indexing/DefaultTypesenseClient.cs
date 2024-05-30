using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Search;

using Typesense;

namespace Kentico.Xperience.Typesense.Collectioning;

/// <summary>
/// Default implementation of <see cref="IXperienceTypesenseClient"/>.
/// </summary>
internal class DefaultTypesenseClient : IXperienceTypesenseClient
{
    private readonly ITypesenseCollectionService typesenseCollectionService;
    private readonly IInfoProvider<ContentLanguageInfo> languageProvider;
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IConversionService conversionService;
    private readonly IContentQueryExecutor executor;
    private readonly ITypesenseClient typesenseClient;
    private readonly IEventLogService eventLogService;
    private readonly IProgressiveCache cache;
    private readonly ITypesenseClient searchClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTypesenseClient"/> class.
    /// </summary>
    public DefaultTypesenseClient(
        ITypesenseCollectionService typesenseCollectionService,
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        ITypesenseClient searchClient,
        IContentQueryExecutor executor,
        ITypesenseClient typesenseClient,
        IEventLogService eventLogService)
    {
        this.typesenseCollectionService = typesenseCollectionService;
        this.cache = cache;
        this.searchClient = searchClient;
        this.executor = executor;
        this.typesenseClient = typesenseClient;
        this.eventLogService = eventLogService;
        this.languageProvider = languageProvider;
        this.channelProvider = channelProvider;
        this.conversionService = conversionService;
    }

    /// <inheritdoc />
    public Task<int> DeleteRecords(IEnumerable<string> itemGuids, string collectionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        if (itemGuids == null || !itemGuids.Any())
        {
            return Task.FromResult(0);
        }

        return DeleteRecordsInternal(itemGuids, collectionName);
    }
    /// <inheritdoc/>
    public async Task<ICollection<TypesenseCollectionStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await searchClient.RetrieveCollections(cancellationToken);

            return stats.Select(i => new TypesenseCollectionStatisticsViewModel
            {
                Name = i.Name,
                NumberOfDocuments = i.NumberOfDocuments,
                UpdatedAt = DateTime.Now // TODO : Change it to the actual value and add extra stats
            })
            .ToList();
        }
        catch (Exception ex)
        {
            eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(GetStatistics)}", "Cannot get the statistics from typesense", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public Task Rebuild(string collectionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        var typesenseCollection = TypesenseCollectionStore.Instance.GetRequiredCollection(collectionName);

        return RebuildInternal(typesenseCollection, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCollection(string collectionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        await searchClient.DeleteCollection(collectionName);
    }

    /// <inheritdoc />
    public Task<int> UpsertRecords(IEnumerable<TypesenseSearchResultModel> dataObjects, string collectionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        if (dataObjects == null || !dataObjects.Any())
        {
            return Task.FromResult(0);
        }

        return UpsertRecordsInternal(dataObjects, collectionName, cancellationToken);
    }

    private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string collectionName)
    {
        int deletedCount = 0;
        //Bulk delete per page of 20 objectIds in parralel
        Parallel.ForEach(objectIds.Chunk(20), async page =>
        {
            string idsToDelete = string.Join(",", page);
            var batchCollectioningResponse = await searchClient.DeleteDocuments(collectionName, $"{BaseObjectProperties.OBJECT_ID}:[{idsToDelete}]");
            Interlocked.Add(ref deletedCount, batchCollectioningResponse.NumberOfDeleted);
        });

        return deletedCount;
    }

    private async Task RebuildInternal(TypesenseCollection typesenseCollection, CancellationToken cancellationToken)
    {
        var indexedItems = new List<CollectionEventWebPageItemModel>();
        foreach (var includedPathAttribute in typesenseCollection.IncludedPaths)
        {
            foreach (string language in typesenseCollection.LanguageNames)
            {
                var queryBuilder = new ContentItemQueryBuilder();

                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    foreach (var contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilder.ForContentType(contentType.ContentTypeName, config => config.ForWebsite(typesenseCollection.WebSiteChannelName, includeUrlPath: true));
                    }
                }
                queryBuilder.InLanguage(language);

                var webpages = await executor.GetWebPageResult(queryBuilder, container => container, cancellationToken: cancellationToken);

                foreach (var page in webpages)
                {
                    var item = await MapToEventItem(page);
                    indexedItems.Add(item);
                }
            }
        }
        await searchClient.DeleteDocuments(typesenseCollection.CollectionName, $"{BaseObjectProperties.OBJECT_ID}: &gt;= 0");

        indexedItems.ForEach(node => TypesenseQueueWorker.EnqueueTypesenseQueueItem(new TypesenseQueueItem(node, TypesenseTaskType.PUBLISH_INDEX, typesenseCollection.CollectionName)));
    }

    private async Task<CollectionEventWebPageItemModel> MapToEventItem(IWebPageContentQueryDataContainer content)
    {
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? "";

        var websiteChannels = await GetAllWebsiteChannels();

        string channelName = websiteChannels.FirstOrDefault(c => c.WebsiteChannelID == content.WebPageItemWebsiteChannelID).ChannelName ?? "";

        var item = new CollectionEventWebPageItemModel(
            content.WebPageItemID,
            content.WebPageItemGUID,
            languageName,
            content.ContentTypeName,
            content.WebPageItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID,
            channelName,
            content.WebPageItemTreePath,
            content.WebPageItemTreePath.Skip(1).Contains('/') ? content.WebPageItemParentID : 0, //the Skip contains avoid excepion on root node
            content.WebPageItemOrder,
            string.Empty // TODO : Add URL
            );

        return item;
    }

    private async Task<int> UpsertRecordsInternal(IEnumerable<TypesenseSearchResultModel> dataObjects, string collectionName, CancellationToken cancellationToken)
    {
        int upsertedCount = 0;
        //await typesenseCollectionService.InitializeCollection(collectionName, cancellationToken);

        foreach (var item in dataObjects) //TODO : Test in parralele mode but be carrefull about the counter
        {
            try
            {
                await typesenseClient.UpsertDocument(collectionName, item);
                upsertedCount++;
            }
            catch (Exception ex)
            {
                eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(UpsertRecordsInternal)}", $"Error when indexing the item (guid): {item.ItemGuid} in language: {item.LanguageName}", ex);
            }
        }

        return upsertedCount;
    }

    private Task<IEnumerable<ContentLanguageInfo>> GetAllLanguages() =>
        cache.LoadAsync(async cs =>
        {
            var results = await languageProvider.Get().GetEnumerableTypedResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");

            return results;
        }, new CacheSettings(5, nameof(DefaultTypesenseClient), nameof(GetAllLanguages)));

    private Task<IEnumerable<(int WebsiteChannelID, string ChannelName)>> GetAllWebsiteChannels() =>
        cache.LoadAsync(async cs =>
        {

            var results = await channelProvider.Get()
                .Source(s => s.Join<WebsiteChannelInfo>(nameof(ChannelInfo.ChannelID), nameof(WebsiteChannelInfo.WebsiteChannelChannelID)))
                .Columns(nameof(WebsiteChannelInfo.WebsiteChannelID), nameof(ChannelInfo.ChannelName))
                .GetDataContainerResultAsync();

            cs.GetCacheDependency = () => CacheHelper.GetCacheDependency(new[] { $"{ChannelInfo.OBJECT_TYPE}|all", $"{WebsiteChannelInfo.OBJECT_TYPE}|all" });

            var items = new List<(int WebsiteChannelID, string ChannelName)>();

            foreach (var item in results)
            {
                if (item.TryGetValue(nameof(WebsiteChannelInfo.WebsiteChannelID), out object channelID) && item.TryGetValue(nameof(ChannelInfo.ChannelName), out object channelName))
                {
                    items.Add(new(conversionService.GetInteger(channelID, 0), conversionService.GetString(channelName, "")));
                }
            }

            return items.AsEnumerable();
        }, new CacheSettings(5, nameof(DefaultTypesenseClient), nameof(GetAllWebsiteChannels)));
}
