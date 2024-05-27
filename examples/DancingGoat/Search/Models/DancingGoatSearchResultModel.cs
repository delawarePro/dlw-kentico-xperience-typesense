using Kentico.Xperience.Typesense.Search;

namespace DancingGoat.Search.Models;

public class DancingGoatSearchResultModel : TypesenseSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
