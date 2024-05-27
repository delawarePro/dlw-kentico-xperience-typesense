using Typesense;

namespace Kentico.Xperience.Typesense.Indexing;

public class TypesenseCollectionSettings : ITypesenseCollectionSettings
{
    public List<Field> Fields { get; init; } = [];
    public string? DefaultSortingField { get; init; }
    public IEnumerable<string>? TokenSeparators { get; init; }
    public IEnumerable<string>? SymbolsToIndex { get; init; }
    public bool? EnableNestedFields { get; init; }

    public Schema ToSchema(string name) => new(name, Fields)
    {
        DefaultSortingField = DefaultSortingField,
        EnableNestedFields = EnableNestedFields,
        SymbolsToIndex = SymbolsToIndex,
        TokenSeparators = TokenSeparators
    };
}

