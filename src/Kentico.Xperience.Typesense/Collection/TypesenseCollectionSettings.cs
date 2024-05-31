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
    public UpdateSchema ToUpdateSchema(string name, IReadOnlyCollection<Field> currentFields)
    {
        var updatedFields = new List<UpdateSchemaField>();
        foreach (var currentField in currentFields)
        {
            var newField = Fields.FirstOrDefault(f => f.Name == currentField.Name);
            if (newField == null) //The field doesn't not exist anymore
            {
                updatedFields.Add(new UpdateSchemaField(currentField.Name, true));
            }
            else
            {
                if (newField.Name != currentField.Name ||
                        newField.Facet != currentField.Facet ||
                        newField.Index != currentField.Index ||
                        newField.Infix != currentField.Infix ||
                        newField.Embed != currentField.Embed ||
                        newField.Locale != currentField.Locale ||
                        newField.NumberOfDimensions != currentField.NumberOfDimensions ||
                        newField.Optional != currentField.Optional ||
                        newField.Reference != currentField.Reference ||
                        newField.Sort != currentField.Sort ||
                        newField.Type != currentField.Type)
                {
                    updatedFields.Add(new UpdateSchemaField(newField.Name, true) //The old field values will be dropped
                    {
                        Facet = newField.Facet,
                        Index = newField.Index,
                        Infix = newField.Infix,
                        Embed = newField.Embed,
                        Locale = newField.Locale,
                        NumberOfDimensions = newField.NumberOfDimensions,
                        Optional = newField.Optional,
                        Reference = newField.Reference,
                        Sort = newField.Sort,
                        Type = newField.Type
                    });
                }
            }
        }

        foreach (var item in Fields)
        {

        }


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

