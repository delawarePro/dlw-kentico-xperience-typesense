namespace Kentico.Xperience.Typesense.Queries;

public class TypesenseQueryModel
{
    public int Id { get; set; }

    public string QueryName { get; set; } = "";

    public string CollectionAlias { get; set; } = "";

    public IEnumerable<TypesenseQueryFieldWeight> FieldWeights { get; set; } = new List<TypesenseQueryFieldWeight>();
}
