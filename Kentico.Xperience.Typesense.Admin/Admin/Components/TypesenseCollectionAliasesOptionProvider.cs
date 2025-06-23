using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.Admin.Components;

public class TypesenseCollectionAliasesOptionProvider(IXperienceTypesenseClient xperienceTypesenseClient) : IDropDownOptionsProvider
{
    public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
        => (await xperienceTypesenseClient.GetAliases())
                        .Select(alias => new DropDownOptionItem
                        {
                            Text = alias.Name,
                            Value = alias.Name
                        });
}
