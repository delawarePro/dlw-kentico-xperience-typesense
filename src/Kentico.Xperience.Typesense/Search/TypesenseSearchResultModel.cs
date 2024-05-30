using System.Text.Json.Serialization;

namespace Kentico.Xperience.Typesense.Search;

public class TypesenseSearchResultModel
{
    [JsonPropertyName("ItemGuid")]
    public Guid ItemGuid { get; set; }
    [JsonPropertyName("ContentTypeName")]
    public string ContentTypeName { get; set; } = "";
    [JsonPropertyName("LanguageName")]
    public string LanguageName { get; set; } = "";
    [JsonPropertyName("objectID")]
    public string ObjectID { get; set; } = "";
    [JsonPropertyName("Url")]
    public string Url { get; set; } = "";

    public TypesenseSearchResultModel()
    {
    }
}
