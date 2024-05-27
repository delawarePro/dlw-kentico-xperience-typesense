using DancingGoat.Search.Models;

using Kentico.Xperience.Typesense.Search;

using Typesense;

namespace DancingGoat.Search.Services;

public class AdvancedSearchService
{
    private readonly ITypesenseClient typesenseClient;

    public AdvancedSearchService(ITypesenseClient typesenseClient) => this.typesenseClient = typesenseClient;

    public async Task<XperienceSearchResult<DancingGoatSearchResultModel>> GlobalSearch(
        string indexName,
        string searchText,
        int page = 1,
        int pageSize = 10,
        string facet = null)
    {
        page = Math.Max(page, 1);

        var searchParameters = new SearchParameters(searchText)
        {
            Page = page - 1,
            PerPage = pageSize
        };

        if (facet is not null)
        {
            searchParameters.FacetBy = nameof(DancingGoatSearchResultModel.ContentTypeName);
        }

        var results = await typesenseClient.Search<DancingGoatSearchResultModel>(indexName, searchParameters);
        return new XperienceSearchResult<DancingGoatSearchResultModel>(searchText, results);
    }
}
