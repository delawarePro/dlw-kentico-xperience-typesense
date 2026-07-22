using CMS.DataEngine;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;

namespace Kentico.Xperience.Typesense.Admin.Providers;

internal class ContentTypeOptionsProvider : IGeneralSelectorDataProvider
{

    public async Task<PagedSelectListItems<string>> GetItemsAsync(string searchTerm, int pageIndex, CancellationToken cancellationToken)
    {
        var itemQuery = DataClassInfoProvider.ProviderObject.Get()
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.CONTENT_TYPE);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            itemQuery.WhereStartsWith(nameof(DataClassInfo.ClassDisplayName), searchTerm);
        }

        itemQuery.Page(pageIndex, 20);

        var items = (await itemQuery.GetEnumerableTypedResultAsync()).Select(x => new ObjectSelectorListItem<string>()
        {
            Value = x.ClassName,
            Text = x.ClassDisplayName,
            IsValid = true
        });

        return new PagedSelectListItems<string>()
        {
            NextPageAvailable = itemQuery.NextPageAvailable,
            Items = items
        };
    }

    public async Task<IEnumerable<ObjectSelectorListItem<string>>> GetSelectedItemsAsync(IEnumerable<string> selectedValues, CancellationToken cancellationToken)
    {
        if (selectedValues == null || !selectedValues.Any())
        {
            return Enumerable.Empty<ObjectSelectorListItem<string>>();
        }

        var itemQuery = DataClassInfoProvider.ProviderObject.Get()
            .WhereEquals(nameof(DataClassInfo.ClassType), ClassType.CONTENT_TYPE)
            .WhereIn(nameof(DataClassInfo.ClassName), selectedValues.ToArray());

        var items = (await itemQuery.GetEnumerableTypedResultAsync()).Select(x => new ObjectSelectorListItem<string>()
        {
            Value = x.ClassName,
            Text = x.ClassDisplayName,
            IsValid = true
        });

        return items;
    }
}
