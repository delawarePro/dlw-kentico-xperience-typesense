using Kentico.Xperience.Typesense.Search;
namespace Kentico.Xperience.Typesense.Collection;

public interface ITypesenseCollectionStrategy
{
    /// <summary>
    /// Called when indexing a search model. Enables overriding of multiple fields with custom data.
    /// </summary>
    /// <param name="typesensePageItem">The <see cref="ICollectionEventItemModel"/> currently being indexed.</param>
    /// <returns>Modified Typesense document.</returns>
    Task<IEnumerable<TypesenseSearchResultModel>?> MapToTypesenseObjectsOrNull(ICollectionEventItemModel typesensePageItem);

    Task<ITypesenseCollectionSettings> GetTypesenseCollectionSettings();

    Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventWebPageItemModel changedItem);

    Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventReusableItemModel changedItem);
}
