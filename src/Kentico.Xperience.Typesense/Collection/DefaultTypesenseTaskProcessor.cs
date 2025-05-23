﻿using CMS.Core;
using CMS.Websites;

using Kentico.Xperience.Typesense.Search;

using Microsoft.Extensions.DependencyInjection;

using Typesense;

namespace Kentico.Xperience.Typesense.Collection;

internal class DefaultTypesenseTaskProcessor : ITypesenseTaskProcessor
{
    private readonly IXperienceTypesenseClient typesenseClient;
    private readonly IServiceProvider serviceProvider;
    private readonly IEventLogService eventLogService;
    private readonly IWebPageUrlRetriever urlRetriever;

    public DefaultTypesenseTaskProcessor(IXperienceTypesenseClient typesenseClient,
        IEventLogService eventLogService,
        IWebPageUrlRetriever urlRetriever,
        IServiceProvider serviceProvider)
    {
        this.typesenseClient = typesenseClient;
        this.eventLogService = eventLogService;
        this.serviceProvider = serviceProvider;
        this.urlRetriever = urlRetriever;
    }

    /// <inheritdoc />
    public async Task<int> ProcessTypesenseTasks(IEnumerable<TypesenseQueueItem> queueItems, CancellationToken cancellationToken)
    {
        int successfulOperations = 0;

        // Group queue items based on index name except the end of rebuild that we will handle at the end
        var groups = queueItems
            .Where(item => item.TaskType != TypesenseTaskType.END_OF_REBUILD)
            .GroupBy(item => item.CollectionName);

        foreach (var group in groups)
        {
            try
            {
                var deleteIds = new List<string>();
                var deleteTasks = group.Where(queueItem => queueItem.TaskType == TypesenseTaskType.DELETE).ToList();

                var updateTasks = group.Where(queueItem => queueItem.TaskType is TypesenseTaskType.PUBLISH_INDEX or TypesenseTaskType.UPDATE);

                var upsertData = new List<TypesenseSearchResultModel>();
                var updateData = new List<TypesenseSearchResultModel>();
                foreach (var queueItem in updateTasks)
                {
                    var documents = await GetDocument(queueItem);
                    if (documents is not null)
                    {
                        foreach (var document in documents)
                        {
                            if (queueItem.TaskType is TypesenseTaskType.UPDATE)
                            {
                                updateData.Add(document);
                            }
                            else
                            {
                                upsertData.Add(document);
                            }
                        }
                    }
                    else
                    {
                        deleteTasks.Add(queueItem);
                    }
                }
                deleteIds.AddRange(
                    GetIdsToDelete(deleteTasks ?? [])
                        .Where(x => x is not null)
                        .Select(x => x ?? ""));

                successfulOperations += await typesenseClient.DeleteRecords(deleteIds, group.Key, cancellationToken);
                successfulOperations += await typesenseClient.UpsertRecords(upsertData, group.Key, ImportType.Create, cancellationToken);
                successfulOperations += await typesenseClient.UpsertRecords(updateData, group.Key, ImportType.Update, cancellationToken);
            }
            catch (Exception ex)
            {
                eventLogService.LogError(nameof(DefaultTypesenseClient), nameof(ProcessTypesenseTasks), ex.Message);
            }
        }

        var endOfQueueItems = queueItems.Where(queueItem => queueItem.TaskType == TypesenseTaskType.END_OF_REBUILD);
        successfulOperations += await typesenseClient.SwapAliasWhenRebuildIsDone(endOfQueueItems, cancellationToken);

        return successfulOperations;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TypesenseSearchResultModel>?> GetDocument(TypesenseQueueItem queueItem)
    {
        if (queueItem.ItemToCollection is null)
        {
            return null;
        }

        var typesenseCollection = TypesenseCollectionStore.Instance.GetRequiredCollection(queueItem.CollectionName);

        var typesenseStrategy = serviceProvider.GetRequiredStrategy(typesenseCollection);
        var data = await typesenseStrategy.MapToTypesenseObjectsOrNull(queueItem.ItemToCollection);
        if (data is not null)
        {
            foreach (var item in data)
            {
                await AddBaseProperties(queueItem.ItemToCollection, item);
            }
        }

        return data;
    }

    private async Task AddBaseProperties(ICollectionEventItemModel baseItem, TypesenseSearchResultModel item)
    {
        if (item is not null && baseItem is not null)
        {
            if (item.ItemGuid == Guid.Empty)
            {
                item.ItemGuid = baseItem.ItemGuid;
            }
            if (string.IsNullOrEmpty(item.ItemName))
            {
                item.ItemName = baseItem.Name;
            }
            if (string.IsNullOrEmpty(item.ContentTypeName))
            {
                item.ContentTypeName = baseItem.ContentTypeName;
            }
            if (string.IsNullOrEmpty(item.LanguageName))
            {
                item.LanguageName = baseItem.LanguageName;
            }

            if (baseItem is CollectionEventWebPageItemModel webpageItem && string.IsNullOrEmpty(item.Url))
            {
                try
                {
                    item.Url = (await urlRetriever.Retrieve(webpageItem.WebPageItemTreePath, webpageItem.WebsiteChannelName, webpageItem.LanguageName)).RelativePath;
                }
                catch (Exception)
                {
                    // Retrieve can throw an exception when processing a page update TypesenseQueueItem
                    // and the page was deleted before the update task has processed. In this case, upsert an
                    // empty URL
                    item.Url = string.Empty;
                }
            }
        }
    }

    private static IEnumerable<string?> GetIdsToDelete(IEnumerable<TypesenseQueueItem> deleteTasks)
        => deleteTasks.Select(queueItem => queueItem?.ItemToCollection?.ItemGuid.ToString() ?? Guid.Empty.ToString());
}
