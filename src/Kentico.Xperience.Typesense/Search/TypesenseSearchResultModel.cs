using System.Text.Json.Serialization;

using Kentico.Xperience.Typesense.JsonConverter;

namespace Kentico.Xperience.Typesense.Search;

[JsonConverter(typeof(TypesenseSearchResultModelConverter))]
public class TypesenseSearchResultModel
{
    [JsonPropertyName("ItemGuid")]
    public Guid ItemGuid { get; set; }
    [JsonPropertyName("ContentTypeName")]
    public string ContentTypeName { get; set; } = "";
    [JsonPropertyName("LanguageName")]
    public string LanguageName { get; set; } = "";
    [JsonPropertyName("ObjectID")]
    public string ObjectID { get; set; } = "";
    [JsonPropertyName("Url")]
    public string Url { get; set; } = "";

    public TypesenseSearchResultModel()
    {
    }
}
