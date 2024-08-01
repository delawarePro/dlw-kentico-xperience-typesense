using Kentico.Xperience.Typesense.Search;

using Typesense;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Default indexing startegy just implements the methods but does not change the data.
/// </summary>
public class DefaultTypesenseCollectionStrategy : ITypesenseCollectionStrategy
{
    /// <inheritdoc />
    public virtual Task<IEnumerable<TypesenseSearchResultModel>?> MapToTypesenseObjectsOrNull(ICollectionEventItemModel typesensePageItem)
    {
        if (typesensePageItem.IsSecured)
        {
            return Task.FromResult<IEnumerable<TypesenseSearchResultModel>?>(null);
        }

        var result = new List<TypesenseSearchResultModel>()
        {
            new( typesensePageItem.ItemID.ToString()) {
                ItemGuid = typesensePageItem.ItemGuid,
                ContentTypeName = typesensePageItem.ContentTypeName,
                LanguageName = typesensePageItem.LanguageName,
                Url = string.Empty // TODO : handle the urls
            }
        };

        return Task.FromResult<IEnumerable<TypesenseSearchResultModel>?>(result);
    }

    public virtual Task<ITypesenseCollectionSettings> GetTypesenseCollectionSettings() =>
        Task.FromResult<ITypesenseCollectionSettings>(new TypesenseCollectionSettings()
        {
            Fields = new(){
            new Field(BaseObjectProperties.ITEM_GUID, FieldType.String),
            new Field(BaseObjectProperties.CONTENT_TYPE_NAME, FieldType.String),
            new Field(BaseObjectProperties.LANGUAGE_NAME, FieldType.String),
            new Field(BaseObjectProperties.URL, FieldType.String),
            }
        });

    public virtual async Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventWebPageItemModel changedItem) => await Task.FromResult(new List<CollectionEventWebPageItemModel>() { changedItem });

    public virtual async Task<IEnumerable<ICollectionEventItemModel>> FindItemsToReindex(CollectionEventReusableItemModel changedItem) => await Task.FromResult(new List<CollectionEventWebPageItemModel>());
}
