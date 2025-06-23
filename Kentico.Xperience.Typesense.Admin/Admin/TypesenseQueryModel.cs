using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;

namespace Kentico.Xperience.Typesense.Admin;

public class TypesenseQueryModel
{
    public int Id { get; set; }

    [TextInputComponent(
       Label = "Collection Alias",
       ExplanationText = "The collection alias where the query should be applied to",
       Order = 1)]
    [Required]
    [MinLength(1)]
    public string CollectionAlias { get; set; } = "";
}
