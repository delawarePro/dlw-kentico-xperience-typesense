using Kentico.Xperience.Typesense.Search;
namespace Kentico.Xperience.Typesense.Collection;

public interface ITypesenseCollectionStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data.
    /// </summary>
    /// <param name="typesensePageItem">The <see cref="ICollectionEventItemModel"/> currently being indexed.</param>
    /// <returns>Modified Typesense document.</returns>
    public Task<IEnumerable<TypesenseSearchResultModel>?> MapToTypesenseObjectsOrNull(ICollectionEventItemModel typesensePageItem);

    public Task<ITypesenseCollectionSettings> GetTypesenseCollectionSettings(bool enableNestedFields = false);

    public Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventWebPageItemModel changedItem);

    public Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventReusableItemModel changedItem);
}
