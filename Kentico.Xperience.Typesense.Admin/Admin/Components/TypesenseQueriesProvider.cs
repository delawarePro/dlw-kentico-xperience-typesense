using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Queries;

namespace Kentico.Xperience.Typesense.Admin.Components;

public class TypesenseQueriesProvider(ITypesenseQueryStorageService typesenseQueryStorageService) : IDropDownOptionsProvider
{
    public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems()
        => typesenseQueryStorageService.GetAllQueryBasicInfo()
                        .Select(x => new DropDownOptionItem
                        {
                            Text = x.TypesenseQueryItemQueryName,
                            Value = x.TypesenseQueryItemQueryName
                        });
}
