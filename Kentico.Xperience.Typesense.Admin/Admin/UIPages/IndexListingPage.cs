using CMS.Core;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;
using Kentico.Xperience.Typesense.Xperience;

[assembly: UIPage(
   parentType: typeof(TypesenseApplicationPage),
   slug: "indexes",
   uiPageType: typeof(CollectionListingPage),
   name: "List of registered Typesense indices",
   templateName: TemplateNames.LISTING,
   order: UIPageOrder.First)]

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// An admin UI page that displays statistics about the registered Typesense indexes.
/// </summary>
[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class CollectionListingPage : ListingPage
{
    private readonly IXperienceTypesenseClient xperienceTypesenseClient;
    private readonly IPageUrlGenerator pageUrlGenerator;
    private readonly ITypesenseConfigurationKenticoStorageService configurationStorageService;
    private readonly IConversionService conversionService;
    protected override string ObjectType => TypesenseCollectionItemInfo.OBJECT_TYPE;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionListingPage"/> class.
    /// </summary>
    public CollectionListingPage(
        IXperienceTypesenseClient xperienceTypesenseClient,
        IPageUrlGenerator pageUrlGenerator,
        ITypesenseConfigurationKenticoStorageService configurationStorageService,
        IConversionService conversionService)
    {
        this.xperienceTypesenseClient = xperienceTypesenseClient;
        this.pageUrlGenerator = pageUrlGenerator;
        this.configurationStorageService = configurationStorageService;
        this.conversionService = conversionService;
    }


    /// <inheritdoc/>
    public override async Task ConfigurePage()
    {
        if (!TypesenseCollectionStore.Instance.GetAllCollections().Any())
        {
            PageConfiguration.Callouts =
            [
                new()
                {
                    Headline = "No indexes",
                    Content = "No Typesense indexes registered. See <a target='_blank' href='https://github.com/Kentico/kentico-xperience-typesense'>our instructions</a> to read more about creating and registering Typesense indexes.",
                    ContentAsHtml = true,
                    Type = CalloutType.FriendlyWarning,
                    Placement = CalloutPlacement.OnDesk
                }
            ];
        }

        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId), "ID", defaultSortDirection: SortTypeEnum.Asc, sortable: true)
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemcollectionName), "Name/Alias", sortable: true, searchable: true)
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemChannelName), "Channel", searchable: true, sortable: true)
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemStrategyName), "Collection Strategy", searchable: true, sortable: true)
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId), "Entries", sortable: true)
            .AddColumn(nameof(TypesenseCollectionItemInfo.TypesenseCollectionItemId), "Current Collection", sortable: true);

        PageConfiguration.AddEditRowAction<CollectionEditPage>();
        PageConfiguration.TableActions.AddCommand("Rebuild", nameof(Rebuild), icon: Icons.RotateRight);
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete), "Delete");
        PageConfiguration.HeaderActions.AddLink<CollectionCreatePage>("Create");

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public async Task<INavigateResponse> Delete(int id, CancellationToken cancellationToken)
    {
        var response = NavigateTo(pageUrlGenerator.GenerateUrl<CollectionListingPage>());
        var index = TypesenseCollectionStore.Instance.GetCollection(id);
        if (index == null)
        {
            return response
                .AddErrorMessage(string.Format("Error deleting Typesense index with identifier {0}.", id));
        }
        try
        {
            var collection = configurationStorageService.GetCollectionDataOrNull(id);

            bool res = await configurationStorageService.TryDeleteCollection(id);

            if (res)
            {
                res = await xperienceTypesenseClient.TryDeleteCollection(collection);
                if (res)
                {
                    TypesenseCollectionStore.SetIndicies(configurationStorageService);
                }
            }
            else
            {
                return response
                    .AddErrorMessage(string.Format("Error deleting Typesense index with identifier {0}.", id));
            }

            return response.AddSuccessMessage("Collection deletion in progress. Visit your Typesense dashboard for details about your indexes.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(CollectionListingPage), nameof(Delete), ex);
            return response
               .AddErrorMessage(string.Format("Errors occurred while deleting the '{0}' index. Please check the Event Log for more details.", index.CollectionName));
        }
    }

    /// <summary>
    /// A page command which rebuilds an Typesense index.
    /// </summary>
    /// <param name="id">The ID of the row whose action was performed, which corresponds with the internal
    /// <see cref="TypesenseCollection.Identifier"/> to rebuild.</param>
    /// <param name="cancellationToken">The cancellation token for the action.</param>
    [PageCommand]
    public async Task<ICommandResponse<RowActionResult>> Rebuild(int id, CancellationToken cancellationToken)
    {
        var result = new RowActionResult(false);
        var index = TypesenseCollectionStore.Instance.GetCollection(id);
        if (index == null)
        {
            return ResponseFrom(result)
            .AddErrorMessage(string.Format("Error loading Typesense index with identifier {0}.", id));
        }

        try
        {
            await xperienceTypesenseClient.Rebuild(index.CollectionName, cancellationToken);
            return ResponseFrom(result)
                 .AddSuccessMessage("Collectioning in progress. Visit your Typesense dashboard for details about the indexing process.");
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(CollectionListingPage), nameof(Rebuild), ex);
            return ResponseFrom(result)
               .AddErrorMessage(string.Format("Errors occurred while rebuilding the '{0}' index. Please check the Event Log for more details.", index.CollectionName));
        }
    }

    private (TypesenseCollectionStatisticsViewModel?, TypesenseCollectionAliasViewModel?) GetStatistic(Row row, ICollection<TypesenseCollectionStatisticsViewModel> statistics, ICollection<TypesenseCollectionAliasViewModel> aliases)
    {
        int indexID = conversionService.GetInteger(row.Identifier, 0);
        string collectionName = TypesenseCollectionStore.Instance.GetCollection(indexID) is TypesenseCollection index
            ? index.CollectionName
            : "";

        var alias = aliases.FirstOrDefault(a => a.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase)); // Use the alias to retreive the physical collection name

        var stat = statistics.FirstOrDefault(s => string.Equals(s.Name, alias?.Name, StringComparison.OrdinalIgnoreCase));
        if (stat is null)
        {
            stat = statistics.FirstOrDefault(s => string.Equals(s.Name, collectionName, StringComparison.OrdinalIgnoreCase));
        }
        return (stat, alias);
    }

    /// <inheritdoc/>
    protected override async Task<LoadDataResult> LoadData(LoadDataSettings settings, CancellationToken cancellationToken)
    {
        var result = await base.LoadData(settings, cancellationToken);

        var statistics = await xperienceTypesenseClient.GetStatistics(cancellationToken);
        var aliases = await xperienceTypesenseClient.GetAliases(cancellationToken);

        // Add statistics for indexes that are registered but not created in Typesense
        AddMissingStatistics(ref statistics);

        if (PageConfiguration.ColumnConfigurations is not List<ColumnConfiguration> columns)
        {
            return result;
        }

        int entriesColCollection = columns.FindIndex(c => c.Caption == "Entries");
        int currentCollectionColCollection = columns.FindIndex(c => c.Caption == "Current Collection");

        foreach (var row in result.Rows)
        {
            if (row.Cells is not List<Cell> cells)
            {
                continue;
            }

            var (stats, alias) = GetStatistic(row, statistics, aliases);

            if (stats is null)
            {
                continue;
            }

            if (cells[entriesColCollection] is StringCell entriesCell)
            {
                entriesCell.Value = stats.NumberOfDocuments.ToString();
            }

            if (cells[currentCollectionColCollection] is StringCell updatedCell)
            {
                updatedCell.Value = alias?.Name?.ToString() ?? string.Empty;
            }
        }

        return result;
    }


    private static void AddMissingStatistics(ref ICollection<TypesenseCollectionStatisticsViewModel> statistics)
    {
        foreach (string collectionName in TypesenseCollectionStore.Instance.GetAllCollections().Select(i => i.CollectionName))
        {
            if (!statistics.Any(stat => stat.Name?.Equals(collectionName, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                statistics.Add(new TypesenseCollectionStatisticsViewModel
                {
                    Name = collectionName,
                    NumberOfDocuments = 0
                });
            }
        }
    }
}
