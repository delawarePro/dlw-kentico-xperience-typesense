using DancingGoat.Search.Models;

using Typesense;

namespace DancingGoat.Search.Services;

public class SimpleSearchService
{
    private readonly ITypesenseClient typesenseClient;

    public SimpleSearchService(ITypesenseClient typesenseClient) => this.typesenseClient = typesenseClient;

    public async Task<SearchResult<DancingGoatSimpleSearchResultModel>> GlobalSearch(
        string collectionName,
        string searchText,
        int page = 1,
        int pageSize = 10)
    {
        //var index = await typesenseCollectionService.InitializeCollection(collectionName, default);

        page = Math.Max(page, 1);

        var searchParameters = new SearchParameters(searchText)
        {
            Page = page - 1,
            PerPage = pageSize
        };

        var results = await typesenseClient.Search<DancingGoatSimpleSearchResultModel>(collectionName, searchParameters);
        return results;
    }
}
