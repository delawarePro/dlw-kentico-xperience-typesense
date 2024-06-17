using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

using Kentico.Xperience.Typesense.Search;

using Microsoft.Extensions.DependencyInjection;

using Typesense;
using Kentico.Xperience.Typesense.QueueWorker;
using Kentico.Xperience.Typesense.Xperience;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Default implementation of <see cref="IXperienceTypesenseClient"/>.
/// </summary>
internal class DefaultTypesenseClient : IXperienceTypesenseClient
{
    private readonly IInfoProvider<ContentLanguageInfo> languageProvider;
    private readonly IInfoProvider<ChannelInfo> channelProvider;
    private readonly IConversionService conversionService;
    private readonly IContentQueryExecutor executor;
    private readonly ITypesenseClient typesenseClient;
    private readonly IEventLogService eventLogService;
    private readonly IProgressiveCache cache;
    private readonly ITypesenseClient searchClient;
    private readonly IServiceProvider serviceProvider;
    private readonly ITypesenseQueue queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTypesenseClient"/> class.
    /// </summary>
    public DefaultTypesenseClient(
        IInfoProvider<ContentLanguageInfo> languageProvider,
        IInfoProvider<ChannelInfo> channelProvider,
        IConversionService conversionService,
        IProgressiveCache cache,
        ITypesenseClient searchClient,
        IContentQueryExecutor executor,
        ITypesenseClient typesenseClient,
        IEventLogService eventLogService,
        IServiceProvider serviceProvider,
        ITypesenseQueue queue)
    {
        this.queue = queue;
        this.serviceProvider = serviceProvider;
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
    // TODO : Use the TryDeleteCollection method
    public async Task DeleteCollection(string collectionName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }

        await searchClient.DeleteCollection(collectionName);
    }

    public async Task<bool> TryDeleteCollection(ITypesenseConfigurationModel? configuration)
    {
        if (configuration is not null)
        {
            var primary = await searchClient.DeleteCollection($"{configuration.CollectionName}-primary");
            var secondary = await searchClient.DeleteCollection($"{configuration.CollectionName}-secondary");

            return primary != null || secondary != null;
        }
        return false;

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

        var (activeCollectionName, newCollectionName) = await GetCollectionNames(typesenseCollection.CollectionName);

        await EnsureNewCollection(newCollectionName, typesenseCollection);


        indexedItems.ForEach(async node => await queue.EnqueueTypesenseQueueItem(new TypesenseQueueItem(node, TypesenseTaskType.PUBLISH_INDEX, newCollectionName)));

        queue.EnqueueTypesenseQueueItem(new TypesenseQueueItem(new EndOfRebuildItemModel(activeCollectionName, newCollectionName, typesenseCollection.CollectionName), TypesenseTaskType.END_OF_REBUILD, typesenseCollection.CollectionName));
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

    private static readonly MediaTypeHeaderValue JsonMediaTypeHeaderValue = MediaTypeHeaderValue.Parse($"{MediaTypeNames.Application.Json};charset={Encoding.UTF8.WebName}");
    private static readonly JsonSerializerOptions JsonOptionsCamelCaseIgnoreWritingNull = new()
    {
        //PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //MaxDepth = 3
         
    };

    private async Task<int> UpsertRecordsInternal(IEnumerable<TypesenseSearchResultModel> dataObjects, string collectionName, CancellationToken cancellationToken)
    {
        int upsertedCount = 0;

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

    public async Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration)
    {
        if (configuration is null)
        {
            return false;
        }

        //Create the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(configuration.CollectionName) ?? throw new InvalidOperationException($"Registered index with name '{configuration.CollectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        var createdCollection = await searchClient.CreateCollection(indexSettings.ToSchema($"{configuration.CollectionName}-primary"));
        if (createdCollection == null)
        {
            return false;
        }

        await searchClient.UpsertCollectionAlias(configuration.CollectionName, new CollectionAlias($"{configuration.CollectionName}-primary"));
        return true;
    }



    public async Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration, Func<string, Task> rebuildAction)
    {
        if (configuration is null)
        {
            return false;
        }
        //Update the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(configuration.CollectionName) ?? throw new InvalidOperationException($"Registered index with name '{configuration.CollectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        var currentCollection = await searchClient.RetrieveCollection(configuration.CollectionName); //Search by alias the current collection

        if (currentCollection == null)
        {
            return false;
        }

        var (_, collectionToReCreate) = await GetCollectionNames(configuration.CollectionName);

        if (CheckIfFieldsRequiredARebuild(currentCollection.Fields, indexSettings.Fields))
        {
            await RebuildInternal(typesenseCollection, default); // Rebuild with a zero down time strategy
            return true;
        }
        else
        {
            //TODO : We could change the base properties here
            return true;
        }
    }

    public async Task<bool> EnsureNewCollection(string newCollection, TypesenseCollection typesenseCollection)
    {
        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = typesenseStrategy.GetTypesenseCollectionSettings();

        var allCollections = await searchClient.RetrieveCollections();

        //First delete the old collection with the new name
        if (allCollections.Exists(x => x.Name == newCollection))
        {
            await searchClient.DeleteCollection(newCollection);
        }

        //Then create the new collection with the new schema
        var createdCollection = await searchClient.CreateCollection(indexSettings.ToSchema(newCollection));
        if (createdCollection == null)
        {
            return false;
        }

        return true;
    }

    private bool CheckIfFieldsRequiredARebuild(IReadOnlyCollection<Field> oldFields, List<Field> newFields)
    {
        //TODO : This can be still improved because some changes deoesn't require a rebuild but we need to play with the uopdate schema then.
        foreach (var oldField in oldFields)
        {
            var newField = newFields.Find(x => x.Name == oldField.Name);

            if (newField == null)
            {
                return true;
            }

            if (oldField.Infix != newField.Infix
                || oldField.Index != newField.Index
                || oldField.Type != newField.Type
                || oldField.Facet != newField.Facet
                || oldField.Locale != newField.Locale
                || oldField.NumberOfDimensions != newField.NumberOfDimensions
                || oldField.Optional != newField.Optional
                || oldField.Reference != newField.Reference
                || oldField.Sort != newField.Sort)
            {
                return true;
            }
        }

        return oldFields.Count == newFields.Count;
    }

    public async Task<int> SwapAliasWhenRebuildIsDone(IEnumerable<TypesenseQueueItem> endOfQueueItems, string key, CancellationToken cancellationToken)
    {
        int processed = 0;
        foreach (var item in endOfQueueItems)
        {
            if (item.ItemToCollection is EndOfRebuildItemModel model)
            {
                await SwapAliasWhenRebuildIsDone(model.CollectionAlias, model.RebuildedCollection);
                processed++;
            }
        }
        return processed;
    }
    public async Task SwapAliasWhenRebuildIsDone(string alias, string newCollectionRebuilded)
    {
        if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(newCollectionRebuilded))
        {
            //TODO : log a message
            return;
        }

        await searchClient.UpsertCollectionAlias(alias, new CollectionAlias(newCollectionRebuilded));
    }

    public async Task<(string activeCollectionName, string newCollectionName)> GetCollectionNames(string collectionName)
    {
        var currentCollection = await searchClient.RetrieveCollection(collectionName); //Search by alias the current collection
        if (currentCollection == null)
        {
            return (string.Empty, string.Empty);
        }

        string newCollectionName = $"{collectionName}-primary"; //Default to primary
        if (currentCollection.Name.EndsWith("-primary"))
        {
            newCollectionName = $"{collectionName}-secondary";
        }

        return (currentCollection.Name, newCollectionName);
    }

}
