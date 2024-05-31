using CMS.ContentEngine.Internal;
using CMS.Core;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Typesense extension methods for the <see cref="CollectionEventWebPageItemModel"/> class.
/// </summary>
internal static class CollectionedItemModelExtensions
{
    /// <summary>
    /// Returns true if the node is included in the Typesense index based on the index's defined paths
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="collectionName">The Typesense index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsCollectionedByCollection(this CollectionEventWebPageItemModel indexedItemModel, IEventLogService log, string collectionName, string eventName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }
        if (indexedItemModel is null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(collectionName);
        if (typesenseCollection is null)
        {
            log.LogError(nameof(CollectionedItemModelExtensions), nameof(IsCollectionedByCollection), $"Error loading registered Typesense index '{collectionName}' for event [{eventName}].");
            return false;
        }

        if (!typesenseCollection.LanguageNames.Exists(x => x == indexedItemModel.LanguageName))
        {
            return false;
        }

        return typesenseCollection.IncludedPaths.Any(path =>
        {
            bool matchesContentType = path.ContentTypes.Exists(x => string.Equals(x.ContentTypeName, indexedItemModel.ContentTypeName));

            if (!matchesContentType)
            {
                return false;
            }

            // Supports wildcard matching
            if (path.AliasPath.EndsWith("/%", StringComparison.OrdinalIgnoreCase))
            {
                string pathToMatch = path.AliasPath[..^2];
                var pathsOnPath = TreePathUtils.GetTreePathsOnPath(indexedItemModel.WebPageItemTreePath, true, false).ToHashSet();

                return pathsOnPath.Any(p => p.StartsWith(pathToMatch, StringComparison.OrdinalIgnoreCase));
            }

            return indexedItemModel.WebPageItemTreePath.Equals(path.AliasPath, StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Returns true if the node is included in the Typesense index's allowed
    /// </summary>
    /// <remarks>Logs an error if the search model cannot be found.</remarks>
    /// <param name="indexedItemModel">The node to check for indexing.</param>
    /// <param name="log"></param>
    /// <param name="collectionName">The Typesense index code name.</param>
    /// <param name="eventName"></param>
    /// <exception cref="ArgumentNullException" />
    public static bool IsCollectionedByCollection(this CollectionEventReusableItemModel indexedItemModel, IEventLogService log, string collectionName, string eventName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentNullException(nameof(collectionName));
        }
        if (indexedItemModel == null)
        {
            throw new ArgumentNullException(nameof(indexedItemModel));
        }

        var typesenseCollection = TypesenseCollectionStore.Instance.GetCollection(collectionName);
        if (typesenseCollection == null)
        {
            log.LogError(nameof(CollectionedItemModelExtensions), nameof(IsCollectionedByCollection), $"Error loading registered Typesense index '{collectionName}' for event [{eventName}].");
            return false;
        }

        if (typesenseCollection.LanguageNames.Exists(x => x == indexedItemModel.LanguageName))
        {
            return true;
        }

        return false;
    }
}
