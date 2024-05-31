using Typesense;

namespace Kentico.Xperience.Typesense.Collection;
public interface ITypesenseCollectionSettings
{
    public List<Field> Fields { get; init; }

    public string? DefaultSortingField { get; init; }

    public IEnumerable<string>? TokenSeparators { get; init; }

    public IEnumerable<string>? SymbolsToIndex { get; init; }

    public bool? EnableNestedFields { get; init; }

    Schema ToSchema(string name);
    UpdateSchema ToUpdateSchema(string name, IReadOnlyCollection<Field> fields);
}
