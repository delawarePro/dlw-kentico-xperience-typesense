using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Represents the configuration of an Typesense index.
/// </summary>
public sealed class TypesenseCollection
{
    /// <summary>
    /// An arbitrary ID used to identify the Typesense index in the admin UI.
    /// </summary>
    public int Identifier { get; set; }

    /// <summary>
    /// The code name of the Typesense index.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// The Name of the WebSiteChannel.
    /// </summary>
    public string WebSiteChannelName { get; }

    /// <summary>
    /// The Language used on the WebSite on the Channel which is indexed.
    /// </summary>
    public List<string> LanguageNames { get; }

    /// <summary>
    /// The type of the class which extends <see cref="ITypesenseCollectionStrategy"/>.
    /// </summary>
    public Type TypesenseCollectioningStrategyType { get; }

    internal IEnumerable<TypesenseCollectionIncludedPath> IncludedPaths { get; set; }

    internal TypesenseCollection(TypesenseConfigurationModel indexConfiguration, Dictionary<string, Type> strategies)
    {
        Identifier = indexConfiguration.Id;
        CollectionName = indexConfiguration.CollectionName;
        WebSiteChannelName = indexConfiguration.ChannelName;
        LanguageNames = indexConfiguration.LanguageNames.ToList();
        IncludedPaths = indexConfiguration.Paths;

        var strategy = typeof(DefaultTypesenseCollectionStrategy);

        if (strategies.ContainsKey(indexConfiguration.StrategyName))
        {
            strategy = strategies[indexConfiguration.StrategyName];
        }

        TypesenseCollectioningStrategyType = strategy;
    }
}
