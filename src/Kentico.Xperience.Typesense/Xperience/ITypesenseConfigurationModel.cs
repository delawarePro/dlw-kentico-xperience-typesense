namespace Kentico.Xperience.Typesense.Xperience;

public interface ITypesenseConfigurationModel 
{
    int Id { get; set; }

    string CollectionName { get; set; }

    IEnumerable<string> LanguageNames { get; set; }

    string ChannelName { get; set; }

    string StrategyName { get; set; }

    string RebuildHook { get; set; }

    IEnumerable<TypesenseCollectionIncludedPath> Paths { get; set; }
}
