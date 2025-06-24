using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Admin.Components;
using Kentico.Xperience.Typesense.Queries;

namespace Kentico.Xperience.Typesense.Query;

public class TypesenseQueryAdminModel
{
    public int Id { get; set; }

    [TextInputComponent(
        Label = "Query Name",
        ExplanationText = "The query name will be display when you need to select a query",
        Order = 10)]
    [Required]
    public string QueryName { get; set; } = "";

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

    public TypesenseQueryAdminModel()
    {
    }

    public TypesenseQueryAdminModel(TypesenseQueryModel model)
    {
        Id = model.Id;
        QueryName = model.QueryName;
        CollectionAlias = model.CollectionAlias;
        FieldWeights = model.FieldWeights;
    }

    public TypesenseQueryModel ToModel() => new()
    {
        Id = Id,
        QueryName = QueryName,
        CollectionAlias = CollectionAlias,
        FieldWeights = FieldWeights
    };
}
