using System.Text.Json.Serialization;

namespace Kentico.Xperience.Typesense.Search;

public class TypesenseSearchResultModel
{
    [JsonPropertyName("ItemGuid")]
    public Guid ItemGuid { get; set; }

    [JsonPropertyName("ItemName")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("ContentTypeName")]
    public string ContentTypeName { get; set; } = string.Empty;

    [JsonPropertyName("LanguageName")]
    public string LanguageName { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string ID { get; set; }

    [JsonPropertyName("Url")]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// If you set it to true this document will be deleted from Typesense index
    /// </summary>
    public bool RemoveFromIndex { get; set; } = false;

    public TypesenseSearchResultModel(string id) => ID = id;

}

//Fake class because we need at least one subclass for the json resolver to work
public class FakeTypesenseSearchResultModel : TypesenseSearchResultModel
{
    public FakeTypesenseSearchResultModel(string id) : base(id)
    {
    }
}
