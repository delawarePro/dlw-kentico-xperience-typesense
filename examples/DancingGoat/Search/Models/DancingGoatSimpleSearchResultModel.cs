using System.Text.Json.Serialization;

using Kentico.Xperience.Typesense.Search;

namespace DancingGoat.Search.Models;

public class DancingGoatSimpleSearchResultModel : TypesenseSearchResultModel
{
    [JsonPropertyName("Title")]
    public string Title { get; set; }

    public DancingGoatSimpleSearchResultModel(string id) : base(id)
    {
    }
}
