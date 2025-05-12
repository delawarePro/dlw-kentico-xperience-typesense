using System.Diagnostics;
using System.Text;
using System.Text.Json;

using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Websites;

using Kentico.Xperience.Typesense.JsonResolvers;
using Kentico.Xperience.Typesense.QueueWorker;
using Kentico.Xperience.Typesense.Search;
using Kentico.Xperience.Typesense.Xperience;

using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;

using Typesense;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Default implementation of <see cref="IXperienceTypesenseClient"/>.
/// </summary>
internal class DefaultTypesenseClient : IXperienceTypesenseClient
{
    private static readonly ActivitySource activitySource = new("Kentico.Xperience.Typesense");

    private static readonly TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

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
        using var activity = activitySource.StartActivity($"{nameof(DeleteRecords)}_{collectionName}", ActivityKind.Client);
        activity?.AddTag("typesense.collection", collectionName);
        activity?.AddTag("typesense.operation", "delete_records");
        activity?.AddTag("typesense.items_count", itemGuids?.Count() ?? 0);

        if (string.IsNullOrEmpty(collectionName))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "CollectionName is null or empty");
            throw new ArgumentNullException(nameof(collectionName));
        }

        if (itemGuids == null || !itemGuids.Any())
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "No items to delete");
            return Task.FromResult(0);
        }

        return DeleteRecordsInternal(itemGuids, collectionName);
    }

    /// <inheritdoc/>
    public async Task<ICollection<TypesenseCollectionStatisticsViewModel>> GetStatistics(CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity($"{nameof(GetStatistics)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "get_statistics");

        try
        {
            var stats = await searchClient.RetrieveCollections(cancellationToken);

            var results = stats.Select(i => new TypesenseCollectionStatisticsViewModel
            {
                Name = i.Name,
                NumberOfDocuments = i.NumberOfDocuments
            })
            .ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddTag("typesense.collections_count", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(GetStatistics)}", "Cannot get the statistics from typesense", ex);
            throw;
        }
    }

    public async Task<ICollection<TypesenseCollectionAliasViewModel>> GetAliases(CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity($"{nameof(GetAliases)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "get_aliases");

        try
        {
            var aliases = await searchClient.ListCollectionAliases(cancellationToken);

            var results = aliases.CollectionAliases.Select(x => new TypesenseCollectionAliasViewModel
            {
                Name = x.Name,
                CollectionName = x.CollectionName
            })
            .ToList();

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddTag("typesense.aliases_count", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(GetStatistics)}", "Cannot get the statistics from typesense", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public Task Rebuild(string collectionName, CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity($"{nameof(Rebuild)}_{collectionName}", ActivityKind.Client);
        activity?.AddTag("typesense.collection", collectionName);
        activity?.AddTag("typesense.operation", "rebuild");

        if (string.IsNullOrEmpty(collectionName))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "CollectionName is null or empty");
            throw new ArgumentNullException(nameof(collectionName));
        }

        var typesenseCollection = TypesenseCollectionStore.Instance.GetRequiredCollection(collectionName);

        return RebuildInternal(typesenseCollection, cancellationToken);
    }

    /// <inheritdoc />
    // TODO : Use the TryDeleteCollection method
    public async Task DeleteCollection(string collectionName, CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity($"{nameof(DeleteCollection)}_{collectionName}", ActivityKind.Client);
        activity?.AddTag("typesense.collection", collectionName);
        activity?.AddTag("typesense.operation", "delete_collection");

        if (string.IsNullOrEmpty(collectionName))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "CollectionName is null or empty");
            throw new ArgumentNullException(nameof(collectionName));
        }

        try
        {
            await searchClient.DeleteCollection(collectionName);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task<bool> TryDeleteCollection(ITypesenseConfigurationModel? configuration)
    {
        using var activity = activitySource.StartActivity($"{nameof(TryDeleteCollection)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "try_delete_collection");

        if (configuration is not null)
        {
            activity?.AddTag("typesense.configuration_name", configuration.CollectionName);

            try
            {
                var primary = await searchClient.DeleteCollection($"{configuration.CollectionName}-primary");
                var secondary = await searchClient.DeleteCollection($"{configuration.CollectionName}-secondary");

                bool result = primary != null || secondary != null;
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.AddTag("typesense.success", result);
                return result;
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                throw;
            }
        }

        activity?.SetStatus(ActivityStatusCode.Ok, "Configuration is null");
        return false;
    }

    /// <inheritdoc />
    public Task<int> UpsertRecords(List<TypesenseSearchResultModel> dataObjects, string collectionName, ImportType importType = ImportType.Create, CancellationToken cancellationToken = default)
    {
        using var activity = activitySource.StartActivity($"{nameof(UpsertRecords)}_{collectionName}", ActivityKind.Client);
        activity?.AddTag("typesense.collection", collectionName);
        activity?.AddTag("typesense.operation", "upsert_records");
        activity?.AddTag("typesense.items_count", dataObjects?.Count() ?? 0);

        if (string.IsNullOrEmpty(collectionName))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "CollectionName is null or empty");
            throw new ArgumentNullException(nameof(collectionName));
        }

        if (dataObjects == null || !dataObjects.Any())
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "No items to upsert");
            return Task.FromResult(0);
        }

        return UpsertRecordsInternal(dataObjects, collectionName, importType);
    }

    private async Task<int> DeleteRecordsInternal(IEnumerable<string> objectIds, string collectionName)
    {
        int deletedCount = 0;
        //Bulk delete per page of 20 objectIds in parralel
        Parallel.ForEach(objectIds.Chunk(20), async page =>
        {
            string idsToDelete = string.Join(",", page);
            var batchCollectioningResponse = await searchClient.DeleteDocuments(collectionName, $"{BaseObjectProperties.ID}:[{idsToDelete}]");
            Interlocked.Add(ref deletedCount, batchCollectioningResponse.NumberOfDeleted);
        });

        return deletedCount;
    }

    private async Task RebuildInternal(TypesenseCollection typesenseCollection, CancellationToken cancellationToken)
    {
        var indexedItems = new List<ICollectionEventItemModel>();
        foreach (var includedPathAttribute in typesenseCollection.IncludedPaths)
        {
            foreach (string language in typesenseCollection.LanguageNames)
            {
                var queryBuilderContent = new ContentItemQueryBuilder();
                var queryBuilderPages = new ContentItemQueryBuilder();

                if (includedPathAttribute.ContentTypes != null && includedPathAttribute.ContentTypes.Count > 0)
                {
                    foreach (var contentType in includedPathAttribute.ContentTypes)
                    {
                        queryBuilderContent.ForContentType(contentType.ContentTypeName);
                        queryBuilderPages.ForContentType(contentType.ContentTypeName, config => config.ForWebsite(typesenseCollection.WebSiteChannelName, includeUrlPath: true));
                    }
                }
                queryBuilderContent.InLanguage(language);
                queryBuilderPages.InLanguage(language);

                var webpages = await executor.GetWebPageResult(queryBuilderPages, container => container, cancellationToken: cancellationToken);

                foreach (var page in webpages)
                {
                    var item = await MapToEventPageItem(page);
                    indexedItems.Add(item);
                }

                var items = await executor.GetResult(queryBuilderContent, container => container, cancellationToken: cancellationToken);
                items = items.Where(i => !webpages.Any(p => p.ContentItemGUID == i.ContentItemGUID)); // Remove the webpages that are already indexed
                foreach (var it in items)
                {
                    var item = await MapToEventItem(it);
                    indexedItems.Add(item);
                }
            }
        }

        try
        {
            await searchClient.RetrieveCollection(typesenseCollection.CollectionName, cancellationToken);
        }
        catch (TypesenseApiNotFoundException)
        {
            await TryCreateCollectionInternal(typesenseCollection.CollectionName);
        }

        await searchClient.DeleteDocuments(typesenseCollection.CollectionName, $"{BaseObjectProperties.ID}: &gt;= 0");

        var (activeCollectionName, newCollectionName) = await GetCollectionNames(typesenseCollection.CollectionName);

        await EnsureNewCollection(newCollectionName, typesenseCollection);

        indexedItems.ForEach(async node => await queue.EnqueueTypesenseQueueItem(new TypesenseQueueItem(node, TypesenseTaskType.PUBLISH_INDEX, newCollectionName)));

        queue.EnqueueTypesenseQueueItem(new TypesenseQueueItem(new EndOfRebuildItemModel(activeCollectionName, newCollectionName, typesenseCollection.CollectionName), TypesenseTaskType.END_OF_REBUILD, typesenseCollection.CollectionName));
    }

    private async Task<CollectionEventReusableItemModel> MapToEventItem(IContentQueryDataContainer content)
    {
        using var activity = activitySource.StartActivity($"{nameof(MapToEventItem)}", ActivityKind.Internal);
        activity?.AddTag("typesense.operation", "map_to_event_item");
        activity?.AddTag("typesense.content_id", content.ContentItemID);
        var languages = await GetAllLanguages();

        string languageName = languages.FirstOrDefault(l => l.ContentLanguageID == content.ContentItemCommonDataContentLanguageID)?.ContentLanguageName ?? "";

        var websiteChannels = await GetAllWebsiteChannels();

        var item = new CollectionEventReusableItemModel(
            content.ContentItemID,
            content.ContentItemGUID,
            languageName,
            content.ContentTypeName,
            content.ContentItemName,
            content.ContentItemIsSecured,
            content.ContentItemContentTypeID,
            content.ContentItemCommonDataContentLanguageID
            );

        activity?.SetStatus(ActivityStatusCode.Ok);
        return item;
    }


    private async Task<CollectionEventWebPageItemModel> MapToEventPageItem(IWebPageContentQueryDataContainer content)
    {
        using var activity = activitySource.StartActivity($"{nameof(MapToEventItem)}", ActivityKind.Internal);
        activity?.AddTag("typesense.operation", "map_to_event_item");
        activity?.AddTag("typesense.content_id", content.WebPageItemID);
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

        activity?.SetStatus(ActivityStatusCode.Ok);
        return item;
    }

    private async Task<int> UpsertRecordsInternal(List<TypesenseSearchResultModel> dataObjects, string collectionName, ImportType importType = ImportType.Create)
    {
        using var activity = activitySource.StartActivity($"{nameof(UpsertRecordsInternal)}_{collectionName}", ActivityKind.Internal);
        activity?.AddTag("typesense.operation", "upsert_records_internal");
        activity?.AddTag("typesense.collection", collectionName);
        activity?.AddTag("typesense.import_type", importType.ToString());
        activity?.AddTag("typesense.items_count", dataObjects?.Count() ?? 0);

        if (dataObjects is null)
        {
            return 0;
        }
        try
        {
            // add the details of the data objects to the activity (as a json object) for better tracing

            string dataObjectsJson = JsonSerializer.Serialize(dataObjects, new JsonSerializerOptions { WriteIndented = true, TypeInfoResolver = new TypeSenseTypeResolver() });
            activity?.AddTag("typesense.data_objects", dataObjectsJson);

            var response = await typesenseClient.ImportDocuments(collectionName, dataObjects, importType: importType);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddTag("typesense.imported_count", response.Count);

            string responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            activity?.AddTag("typesense.response", responseJson);

            var errorsMessage = new StringBuilder();
            for (int i = 0; i < response.Count; i++)
            {
                if (!response[i].Success)
                {
                    errorsMessage.AppendLine($"Error in the item {dataObjects[i]?.ItemName} (id: {dataObjects[i]?.ID}) - {response[i].Error}");
                }
            }

            if (errorsMessage.Length > 0)
            {
                activity?.AddTag("typesense.errors", errorsMessage.ToString());
                eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(UpsertRecordsInternal)}", $"Error when indexing item in bulk", new Exception(errorsMessage.ToString()));
            }

            return response.Count;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            eventLogService.LogException($"{nameof(DefaultTypesenseClient)} - {nameof(UpsertRecordsInternal)}", $"Error when indexing item in bulk", ex);
        }

        return 0;
    }

    private Task<IEnumerable<ContentLanguageInfo>> GetAllLanguages() =>
        cache!.LoadAsync(async cs =>
        {
            using var activity = activitySource.StartActivity($"{nameof(GetAllLanguages)}", ActivityKind.Internal);
            activity?.AddTag("typesense.operation", "get_all_languages");

            try
            {
                var results = await languageProvider.Get().GetEnumerableTypedResultAsync();

                cs.GetCacheDependency = () => CacheHelper.GetCacheDependency($"{ContentLanguageInfo.OBJECT_TYPE}|all");

                activity?.AddTag("typesense.languages_count", results?.Count() ?? 0);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return results ?? new List<ContentLanguageInfo>();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                throw;
            }
        }, new CacheSettings(5, nameof(DefaultTypesenseClient), nameof(GetAllLanguages)));

    private Task<IEnumerable<(int WebsiteChannelID, string ChannelName)>> GetAllWebsiteChannels() =>
        cache.LoadAsync(async cs =>
        {
            using var activity = activitySource.StartActivity($"{nameof(GetAllWebsiteChannels)}", ActivityKind.Internal);
            activity?.AddTag("typesense.operation", "get_all_website_channels");

            try
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

                activity?.AddTag("typesense.channels_count", items.Count);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return items.AsEnumerable();
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.RecordException(ex);
                throw;
            }
        }, new CacheSettings(5, nameof(DefaultTypesenseClient), nameof(GetAllWebsiteChannels)));

    public async Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration)
    {
        using var activity = activitySource.StartActivity($"{nameof(TryCreateCollection)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "try_create_collection");

        if (configuration is null)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Configuration is null");
            return false;
        }

        activity?.AddTag("typesense.configuration_name", configuration.CollectionName);
        bool result = await TryCreateCollectionInternal(configuration.CollectionName);
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddTag("typesense.success", result);
        return result;
    }

    private async Task<bool> TryCreateCollectionInternal(string collectionName)
    {
        //Create the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(collectionName) ??
                                  throw new InvalidOperationException(
                                      $"Registered index with name '{collectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = await typesenseStrategy.GetTypesenseCollectionSettings();

        await searchClient.CreateCollection(indexSettings.ToSchema($"{collectionName}-primary"));

        await searchClient.UpsertCollectionAlias(collectionName,
            new CollectionAlias($"{collectionName}-primary"));

        return true;
    }

    public async Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration, Func<string, Task> rebuildAction)
    {
        using var activity = activitySource.StartActivity($"{nameof(TryEditCollection)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "try_edit_collection");

        if (configuration is null)
        {
            activity?.SetStatus(ActivityStatusCode.Ok, "Configuration is null");
            return false;
        }

        activity?.AddTag("typesense.configuration_name", configuration.CollectionName);
        //Update the collection in Typesense
        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(configuration.CollectionName) ?? throw new InvalidOperationException($"Registered index with name '{configuration.CollectionName}' doesn't exist.");

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = await typesenseStrategy.GetTypesenseCollectionSettings();

        var currentCollection = await searchClient.RetrieveCollection(configuration.CollectionName); //Search by alias the current collection

        if (currentCollection == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Collection not found");
            return false;
        }

        var (_, collectionToReCreate) = await GetCollectionNames(configuration.CollectionName);

        if (CheckIfFieldsRequiredARebuild(currentCollection.Fields, indexSettings.Fields))
        {
            activity?.AddTag("typesense.rebuild_required", true);
            await RebuildInternal(typesenseCollection, default); // Rebuild with a zero down time strategy
            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
        else
        {
            activity?.AddTag("typesense.rebuild_required", false);
            //TODO : We could change the base properties here
            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
    }

    public async Task<bool> EnsureNewCollection(string newCollection, TypesenseCollection typesenseCollection)
    {
        using var activity = activitySource.StartActivity($"{nameof(EnsureNewCollection)}_{newCollection}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "ensure_new_collection");
        activity?.AddTag("typesense.collection", newCollection);

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var indexSettings = await typesenseStrategy.GetTypesenseCollectionSettings();

        var allCollections = await searchClient.RetrieveCollections();

        //First delete the old collection with the new name
        if (allCollections.Exists(x => x.Name == newCollection))
        {
            activity?.AddEvent(new ActivityEvent("Deleting existing collection"));
            await searchClient.DeleteCollection(newCollection);
        }

        //Then create the new collection with the new schema
        var createdCollection = await searchClient.CreateCollection(indexSettings.ToSchema(newCollection));
        if (createdCollection == null)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to create collection");
            return false;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
        return true;
    }

    private bool CheckIfFieldsRequiredARebuild(IReadOnlyCollection<Field> oldFields, List<Field> newFields)
    {
        using var activity = activitySource.StartActivity($"{nameof(CheckIfFieldsRequiredARebuild)}", ActivityKind.Internal);
        activity?.AddTag("typesense.operation", "check_rebuild_required");
        activity?.AddTag("typesense.old_fields_count", oldFields.Count);
        activity?.AddTag("typesense.new_fields_count", newFields.Count);

        //TODO : This can be still improved because some changes deoesn't require a rebuild but we need to play with the uopdate schema then.
        foreach (var oldField in oldFields)
        {
            var newField = newFields.Find(x => x.Name == oldField.Name);

            if (newField == null)
            {
                activity?.AddTag("typesense.rebuild_reason", "missing_field");
                activity?.SetStatus(ActivityStatusCode.Ok);
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
                activity?.AddTag("typesense.rebuild_reason", "field_property_changed");
                activity?.AddTag("typesense.field_name", newField.Name);
                activity?.SetStatus(ActivityStatusCode.Ok);
                return true;
            }
        }

        bool result = oldFields.Count == newFields.Count;
        if (result)
        {
            activity?.AddTag("typesense.rebuild_reason", "field_count_mismatch");
        }
        activity?.SetStatus(ActivityStatusCode.Ok);
        return result;
    }

    public async Task<int> SwapAliasWhenRebuildIsDone(IEnumerable<TypesenseQueueItem> endOfQueueItems, CancellationToken cancellationToken)
    {
        using var activity = activitySource.StartActivity($"{nameof(SwapAliasWhenRebuildIsDone)}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "swap_alias_batch");
        activity?.AddTag("typesense.items_count", endOfQueueItems?.Count() ?? 0);

        int processed = 0;
        if (endOfQueueItems is not null)
        {
            foreach (var item in endOfQueueItems)
            {
                if (item.ItemToCollection is EndOfRebuildItemModel model)
                {
                    activity?.AddEvent(new ActivityEvent($"Processing alias swap for {model.CollectionAlias}"));
                    await SwapAliasWhenRebuildIsDone(model.CollectionAlias, model.RebuildedCollection);
                    processed++;
                }
            }
        }
        activity?.SetStatus(ActivityStatusCode.Ok);
        activity?.AddTag("typesense.processed_count", processed);
        return processed;
    }

    public async Task SwapAliasWhenRebuildIsDone(string alias, string newCollectionRebuilded)
    {
        using var activity = activitySource.StartActivity($"{nameof(SwapAliasWhenRebuildIsDone)}_{alias}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "swap_alias");
        activity?.AddTag("typesense.alias", alias);
        activity?.AddTag("typesense.new_collection", newCollectionRebuilded);

        if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(newCollectionRebuilded))
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Alias or new collection name is null or empty");
            //TODO : log a message
            return;
        }

        try
        {
            await searchClient.UpsertCollectionAlias(alias, new CollectionAlias(newCollectionRebuilded));
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }

    public async Task<(string activeCollectionName, string newCollectionName)> GetCollectionNames(string collectionName)
    {
        using var activity = activitySource.StartActivity($"{nameof(GetCollectionNames)}_{collectionName}", ActivityKind.Client);
        activity?.AddTag("typesense.operation", "get_collection_names");
        activity?.AddTag("typesense.collection", collectionName);

        try
        {
            var currentCollection = await searchClient.RetrieveCollection(collectionName); //Search by alias the current collection
            if (currentCollection == null)
            {
                activity?.SetStatus(ActivityStatusCode.Ok, "Collection not found");
                activity?.AddTag("typesense.found", false);
                return (string.Empty, string.Empty);
            }

            string newCollectionName = $"{collectionName}-primary"; //Default to primary
            if (currentCollection.Name.EndsWith("-primary"))
            {
                newCollectionName = $"{collectionName}-secondary";
            }

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddTag("typesense.found", true);
            activity?.AddTag("typesense.active_collection", currentCollection.Name);
            activity?.AddTag("typesense.new_collection", newCollectionName);
            return (currentCollection.Name, newCollectionName);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            throw;
        }
    }
}
