using System.Text.Json.Serialization;

using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseQueryFieldWeightItem;

namespace Kentico.Xperience.Typesense.Queries;

/// <summary>
/// Represents a field weight configuration for Typesense query searches.
/// </summary>
public class TypesenseQueryFieldWeight
{
    /// <summary>
    /// The name of the field to search.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// The weight/importance of this field (default is 1).
    /// </summary>
    public int Weight { get; set; } = 1;

    /// <summary>
    /// The internal identifier of the field weight.
    /// </summary>
    public string? Identifier { get; set; }

    [JsonConstructor]
    public TypesenseQueryFieldWeight(string fieldName) => FieldName = fieldName;

    /// <summary>
    /// Creates a new TypesenseQueryFieldWeight from a query field weight item info.
    /// </summary>
    /// <param name="fieldWeightInfo">The field weight item info.</param>
    public TypesenseQueryFieldWeight(TypesenseQueryFieldWeightItemInfo fieldWeightInfo)
    {
        FieldName = fieldWeightInfo.TypesenseQueryFieldWeightItemFieldName;
        Weight = fieldWeightInfo.TypesenseQueryFieldWeightItemWeight;
        Identifier = fieldWeightInfo.TypesenseQueryFieldWeightItemId.ToString();
    }
}
