
using Typesense;

namespace Kentico.Xperience.Typesense.Search;
public record XperienceSearchResult<T> : SearchResult<T>
{
    public string SearchText { get; set; } = string.Empty;

    public XperienceSearchResult(string searchText, SearchResult<T> searchResult)
        : base(searchResult.FacetCounts, searchResult.Found, searchResult.OutOf, searchResult.Page, searchResult.SearchTimeMs, searchResult.SearchTimeMs, searchResult.Hits)
        => SearchText = searchText;
}
