namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Selects which items of a rebuild are really indexed when the rebuild queries every language of a collection with language fallback.
/// </summary>
internal static class RebuildItemSelector
{
    /// <summary>
    /// A rebuild queries each language of the collection with language fallback, so an item that exists in one
    /// of the collection's languages can also be returned, in a fallback language, by the pass of another language
    /// (e.g. a nl-be item returned as fr-be by the fr-fr pass, because fr-fr falls back to fr-be).
    /// For each content item: if it was found in at least one of the collection's languages, only those versions are kept;
    /// the fallback copies are dropped. If it was found only through fallback, it is kept once per language.
    /// </summary>
    /// <param name="items">The items returned by all the language passes of the rebuild.</param>
    /// <param name="collectionLanguages">The languages the collection is configured with.</param>
    public static List<ICollectionEventItemModel> KeepBestLanguagePerItem(IEnumerable<ICollectionEventItemModel> items, IEnumerable<string> collectionLanguages)
    {
        var languages = new HashSet<string>(collectionLanguages, StringComparer.OrdinalIgnoreCase);
        var result = new List<ICollectionEventItemModel>();

        foreach (var itemVersions in items.GroupBy(item => item.ItemGuid))
        {
            var versions = itemVersions.ToList();
            var inCollectionLanguages = versions.Where(item => languages.Contains(item.LanguageName)).ToList();

            // Several passes can return the same fallback version, so keep one version per language.
            var kept = inCollectionLanguages.Count > 0 ? inCollectionLanguages : versions;
            result.AddRange(kept.GroupBy(item => item.LanguageName, StringComparer.OrdinalIgnoreCase).Select(group => group.First()));
        }

        return result;
    }
}
