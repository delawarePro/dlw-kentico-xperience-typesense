using System.ComponentModel.DataAnnotations;

using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Typesense.Admin.Components;
using Kentico.Xperience.Typesense.Admin.Providers;
using Kentico.Xperience.Typesense.Xperience;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIndexLanguageItem;

namespace Kentico.Xperience.Typesense.Admin;

public class TypesenseConfigurationModel : ITypesenseConfigurationModel
{
    public int Id { get; set; }

    [TextInputComponent(
       Label = "Collection Name",
       ExplanationText = "Changing this value on an existing index without changing application code will cause the search experience to stop working.",
       Order = 1)]
    [Required]
    [MinLength(1)]
    public string CollectionName { get; set; } = "";

    [GeneralSelectorComponent(dataProviderType: typeof(LanguageOptionsProvider),
        Label = "Collectioned Languages",
        Order = 2,
        ExplanationText = "You can choose your strategy but typesense's best practice is to store one language per index")]
    public IEnumerable<string> LanguageNames { get; set; } = Enumerable.Empty<string>();

    [DropDownComponent(Label = "Channel Name", DataProviderType = typeof(ChannelOptionsProvider), Order = 3)]
    public string ChannelName { get; set; } = "";

    [DropDownComponent(Label = "Collectioning Strategy", DataProviderType = typeof(CollectioningStrategyOptionsProvider), Order = 4)]
    public string StrategyName { get; set; } = "";

    //[TextInputComponent(Label = "Rebuild Hook")] //TODO : Hinerited from Algolia implementation but I am really not sure that it work even in the algolia implementation
    public string RebuildHook { get; set; } = "";

    [TypesenseCollectionConfigurationComponent(Label = "Included Paths")]
    public IEnumerable<TypesenseCollectionIncludedPath> Paths { get; set; } = new List<TypesenseCollectionIncludedPath>();

    public TypesenseConfigurationModel() { }

    public TypesenseConfigurationModel(TypesenseIndexItemInfo index,
        IEnumerable<TypesenseIndexLanguageItemInfo> indexLanguages,
        IEnumerable<TypesenseIncludedPathItemInfo> indexPaths,
        IEnumerable<TypesenseCollectionContentType> contentTypes,
        List<TypesenseContentTypeItemInfo> typesenseContentTypeItemInfos)
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
            .Select(p => new TypesenseCollectionIncludedPath(p, contentTypes, typesenseContentTypeItemInfos))
            .ToList();
    }
}
