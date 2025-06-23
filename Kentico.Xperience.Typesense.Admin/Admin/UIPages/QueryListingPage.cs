using CMS.Core;
using CMS.Membership;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Admin.UIPages;

[assembly: UIPage(
   parentType: typeof(TypesenseApplicationPage),
   slug: "queries",
   uiPageType: typeof(QueryListingPage),
   name: "List of registered Typesense queries",
   templateName: TemplateNames.LISTING,
   order: UIPageOrder.NoOrder)]

namespace Kentico.Xperience.Typesense.Admin.UIPages;

/// <summary>
/// An admin UI page that displays registered Typesense queries.
/// </summary>
[UIEvaluatePermission(SystemPermissions.VIEW)]
internal class QueryListingPage : ListingPage
{
    private readonly IPageLinkGenerator pageLinkGenerator;
    private readonly ITypesenseQueryStorageService queryStorageService;
    protected override string ObjectType => TypesenseQueryItemInfo.OBJECT_TYPE;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryListingPage"/> class.
    /// </summary>
    public QueryListingPage(
        IPageLinkGenerator pageLinkGenerator,
        ITypesenseQueryStorageService queryStorageService)
    {
        this.pageLinkGenerator = pageLinkGenerator;
        this.queryStorageService = queryStorageService;
    }

    /// <inheritdoc/>
    public override async Task ConfigurePage()
    {
        if (!queryStorageService.GetAllQueryData().Any())
        {
            PageConfiguration.Callouts =
            [
                new()
                {
                    Headline = "No queries",
                    Content = "No Typesense queries registered. Create a new query to get started.",
                    ContentAsHtml = true,
                    Type = CalloutType.FriendlyWarning,
                    Placement = CalloutPlacement.OnDesk
                }
            ];
        }

        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(TypesenseQueryItemInfo.TypesenseQueryItemId), "ID", defaultSortDirection: SortTypeEnum.Asc, sortable: true)
            .AddColumn(nameof(TypesenseQueryItemInfo.TypesenseQueryItemCollectionAlias), "Collection Alias", sortable: true, searchable: true);

        PageConfiguration.AddEditRowAction<QueryEditPage>();
        PageConfiguration.TableActions.AddDeleteAction(nameof(DeleteQuery), "Delete");
        PageConfiguration.HeaderActions.AddLink<QueryCreatePage>("Create");

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public async Task<INavigateResponse> DeleteQuery(int id)
    {
        var response = NavigateTo(pageLinkGenerator.GetPath<QueryListingPage>());
        
        try
        {
            bool res = await queryStorageService.TryDeleteQuery(id);

            if (res)
            {
                return response.AddSuccessMessage("Query deleted successfully.");
            }
            else
            {
                return response
                    .AddErrorMessage(string.Format("Error deleting Typesense query with identifier {0}.", id));
            }
        }
        catch (Exception ex)
        {
            EventLogService.LogException(nameof(QueryListingPage), nameof(DeleteQuery), ex);
            return response
                .AddErrorMessage(string.Format("Error deleting Typesense query with identifier {0}.", id));
        }
    }
}
