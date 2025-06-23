using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Admin.Components;

namespace Kentico.Xperience.Typesense.Admin;

public class TypesenseQueryModel
{
    public int Id { get; set; }

    [DropDownComponent(
        Label = "Collection Alias",
        ExplanationText = "The collection alias where the query should be applied to",
        DataProviderType = typeof(TypesenseCollectionAliasesOptionProvider),
        Order = 50)]
    [Required]
    public string CollectionAlias { get; set; } = "";

    [TypesenseQueryFieldWeightsComponent(
        Label = "Field Weights",
        ExplanationText = "Configure the fields to search with their respective weights. Higher weights give more importance to matches in that field.",
        Order = 100)]
    public IEnumerable<TypesenseQueryFieldWeight> FieldWeights { get; set; } = new List<TypesenseQueryFieldWeight>();
}
