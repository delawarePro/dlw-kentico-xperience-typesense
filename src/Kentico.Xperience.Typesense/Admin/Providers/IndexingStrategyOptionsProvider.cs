using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.Admin;

internal class CollectioningStrategyOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() =>
    Task.FromResult(StrategyStorage.Strategies.Keys.Select(x => new DropDownOptionItem()
    {
        Value = x,
        Text = x
    }));
}
