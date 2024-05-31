# Search collection querying

## Rebuild the Search collection

Each collection will initially be empty after creation until you create or modify some content.

To collection all existing content, rebuild the collection in Xperience's Administration within the Search application added by this library.

## Create a search result model

```csharp
public class DancingGoatSearchResultModel : AlgoliaSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
```

## Create a search service

Execute a search with a customized Algolia `Query` using the IAlgoliaSearchService. Specify Facets or other special properties which you have defined in `GetAlgoliacollectionSettings` method in the custom AlgoliacollectioningStrategy

```csharp
public class SearchService
{
     private readonly IAlgoliacollectionService algoliaSearchService;

  public SearchService(IAlgoliacollectionService algoliaSearchService) => this.algoliaSearchService = algoliaSearchService;

  public async Task<SearchResponse<DancingGoatSearchResultModel>> GlobalSearch(
      string collectionName,
      string searchText,
      int page = 1,
      int pageSize = 10,
      string facet = null)
  {
      var collection = await algoliaSearchService.Initializecollection(collectionName, default);

      page = Math.Max(page, 1);

      var query = new Query(searchText)
      {
          Page = page - 1,
          HitsPerPage = pageSize
      };
      if (facet is not null)
      {
          query.Facets = new List<string> { nameof(DancingGoatSearchResultModel.ContentTypeName) };
      }

      var results = await collection.SearchAsync<DancingGoatSearchResultModel>(query);

      return results;
  }
}
```

## Display Results

Create a Controller which uses `SearchService` to display view with search bar.

```csharp
[Route("[controller]")]
[ApiController]
public class SearchController : Controller
{
    private readonly SearchService searchService;

    private const string NAME_OF_DEFAULT_collection = "Default";

    public SearchController(SearchService searchService)
    {
        this.searchService = searchService;
    }

    public async Task<IActionResult> collection(string query, int pageSize = 10, int page = 1, string facet = null, string collectionName = null)
    {
        try
        {
            var results = await advancedSearchService.GlobalSearch(collectionName ?? NAME_OF_DEFAULT_collection, query, page, pageSize, facet);
            return View(results);
        }
        catch
        {
            return NotFound();
        }
    }
}
```

The controller retrieves `collection.cshtml` stored in `Views/Search/` solution folder. You can use `GetRouteData` method to assemble the parameters of the url of the endpoint defined in `SearchController`.

```cshtml
@model SearchResponse<DancingGoatSearchResultModel>
@{
    Dictionary<string, string> GetRouteData(int page) =>
        new Dictionary<string, string>() { { "query", Model.Query }, { "pageSize", Model.ToString() }, { "page", page.ToString() } };
}

<h1>Search</h1>

<style>
    .form-field {
        margin-bottom: 0.8rem;
    }
</style>


<div class="row" style="padding: 1rem;">
    <div class="col-12">
        <form asp-action="collection" method="get">
            <div class="row">
                <div class="col-md-12">
                    <div class="form-field">
                        <label class="control-label" asp-for="@Model.Query"></label>
                        <div class="editing-form-control-nested-control">
                            <input class="form-control" asp-for="@Model.Query" name="query">
                            <input type="hidden" asp-for="@Model.Page" name="page" />
                        </div>
                    </div>
                </div>
            </div>

            <input type="submit" value="Submit">
        </form>
    </div>
</div>

@if (!Model.Hits.Any())
{
    if (!String.IsNullOrWhiteSpace(Model.Query))
    {
        @HtmlLocalizer["Sorry, no results match {0}", Model.Query]
    }

    return;
}

@foreach (var item in Model.Hits)
{
    <div class="row search-tile">
        <h3 class="h4 search-tile-title">
            <a href="@item.Url">@item.Title</a>
        </h3>
    </div>
}

<div class="pagination-container">
    <ul class="pagination">
        @if (Model.Page > 1)
        {
            <li class="PagedList-skipToPrevious">
                <a asp-controller="Search" asp-all-route-data="GetRouteData(Model.Page - 1)">
                    @HtmlLocalizer["previous"]
                </a>
            </li>
        }

        @for (int pageNumber = 1; pageNumber <= Model.NbPages; pageNumber++)
        {
            if (pageNumber == Model.Page)
            {
                <li class="active">
                    <span>
                        @pageNumber
                    </span>
                </li>
            }
            else
            {
                <li>
                    <a asp-controller="Search" asp-all-route-data="GetRouteData(pageNumber)">@pageNumber</a>
                </li>
            }
        }

        @if (Model.Page < Model.NbPages)
        {
            <li class="PagedList-skipToNext">
                <a asp-controller="Search" asp-all-route-data="GetRouteData(Model.Page + 1)">
                    @HtmlLocalizer["next"]
                </a>
            </li>
        }
    </ul>
</div>
```
