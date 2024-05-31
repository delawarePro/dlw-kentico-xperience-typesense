using Typesense;

namespace Kentico.Xperience.Typesense.Collection;

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
    public UpdateSchema ToUpdateSchema(string name)
    {
        var readoOnlyFields = (from f in Fields
                               select new UpdateSchemaField(f.Name, false)
                               {
                                   Facet = f.Facet,
                                   Index = f.Index,
                                   Infix = f.Infix,
                                   Embed = f.Embed,
                                   Name = f.Name,
                                   Locale = f.Locale,
                                   NumberOfDimensions = f.NumberOfDimensions,
                                   Optional = f.Optional,
                                   Reference = f.Reference,
                                   Sort = f.Sort,
                                   Type = f.Type
                               })
                               .ToList()
                               .AsReadOnly();

        return new UpdateSchema(readoOnlyFields);
    }
}

