using System.ComponentModel.DataAnnotations;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Admin.Providers;

namespace Kentico.Xperience.Typesense.Admin;

public class TypesenseConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(
       Label = "Collection Name",
       ExplanationText = "Changing this value on an existing index without changing application code will cause the search experience to stop working.",
       Order = 1)]
    [Required]
    [MinLength(1)]
    public string CollectionName { get; set; } = "";

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider), Label = "Collectioned Languages", Order = 2)]
    public IEnumerable<string> LanguageNames { get; set; } = Enumerable.Empty<string>();

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 3)]
    public string ChannelName { get; set; } = "";

    [DropDownComponent(Label = "Collectioning Strategy", DataProviderType = typeof(CollectioningStrategyOptionsProvider), Order = 4)]
    public string StrategyName { get; set; } = "";

    [TextInputComponent(Label = "Rebuild Hook")]
    public string RebuildHook { get; set; } = "";

    [TypesenseCollectionConfigurationComponent(Label = "Included Paths")]
    public IEnumerable<TypesenseCollectionIncludedPath> Paths { get; set; } = new List<TypesenseCollectionIncludedPath>();

    public TypesenseConfigurationModel() { }

    public TypesenseConfigurationModel(
        TypesenseCollectionItemInfo index,
        IEnumerable<TypesenseCollectionLanguageItemInfo> indexLanguages,
        IEnumerable<TypesenseIncludedPathItemInfo> indexPaths,
        IEnumerable<TypesenseCollectionContentType> contentTypes
    )
    {
        Id = index.TypesenseCollectionItemId;
        CollectionName = index.TypesenseCollectionItemcollectionName;
        ChannelName = index.TypesenseCollectionItemChannelName;
        RebuildHook = index.TypesenseCollectionItemRebuildHook;
        StrategyName = index.TypesenseCollectionItemStrategyName;
        LanguageNames = indexLanguages
            .Where(l => l.TypesenseCollectionLanguageItemCollectionItemId == index.TypesenseCollectionItemId)
            .Select(l => l.TypesenseCollectionLanguageItemName)
            .ToList();
        Paths = indexPaths
            .Where(p => p.TypesenseIncludedPathItemCollectionItemId == index.TypesenseCollectionItemId)
            .Select(p => new TypesenseCollectionIncludedPath(p, contentTypes))
            .ToList();
    }
}
