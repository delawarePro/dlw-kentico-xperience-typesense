using Kentico.Xperience.Typesense.Xperience;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Represents a store of Typesense indexes and crawlers.
/// </summary>
public class TypesenseCollectionStore
{
    private static readonly Lazy<TypesenseCollectionStore> mInstance = new();
    private readonly List<TypesenseCollection> registeredCollections = [];

    /// <summary>
    /// Gets current instance of the <see cref="TypesenseCollectionStore"/> class.
    /// </summary>
    public static TypesenseCollectionStore Instance => mInstance.Value;

    /// <summary>
    /// Gets all registered indexes.
    /// </summary>
    public IEnumerable<TypesenseCollection> GetAllCollections() => registeredCollections;

    /// <summary>
    /// Gets a registered <see cref="TypesenseCollection"/> with the specified <paramref name="collectionName"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="collectionName">The name of the index to retrieve.</param>
    /// <param name="allowPhysicalNames">if false this will only search by alias</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public TypesenseCollection? GetCollection(string collectionName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            return null;
        }

        collectionName = RemovePostfix(collectionName);

        return registeredCollections.SingleOrDefault(i => i.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase));
    }

    private string RemovePostfix(string collectionName) => collectionName.Replace("-primary", string.Empty)
                                                                         .Replace("-secondary", string.Empty);

    /// <summary>
    /// Gets a registered <see cref="TypesenseCollection"/> with the specified <paramref name="identifier"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="identifier">The identifier of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public TypesenseCollection? GetCollection(int identifier) => registeredCollections.Find(i => i.Identifier == identifier);

    /// <summary>
    /// Gets a registered <see cref="TypesenseCollection"/> with the specified <paramref name="collectionName"/>. If no index is found, a <see cref="InvalidOperationException" /> is thrown.
    /// </summary>
    /// <param name="collectionName">The name of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public TypesenseCollection GetRequiredCollection(string collectionName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            throw new ArgumentException("Value must not be null or empty");
        }

        collectionName = RemovePostfix(collectionName);

        return registeredCollections.SingleOrDefault(i => i.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"The index '{collectionName}' is not registered.");
    }

    /// <summary>
    /// Adds an index to the store.
    /// </summary>
    /// <param name="index">The index to add.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public void AddCollection(TypesenseCollection index)
    {
        if (index == null)
        {
            throw new ArgumentNullException(nameof(index));
        }

        if (registeredCollections.Exists(i => i.CollectionName.Equals(index.CollectionName, StringComparison.OrdinalIgnoreCase) || index.Identifier == i.Identifier))
        {
            throw new InvalidOperationException($"Attempted to register Typesense index with identifer [{index.Identifier}] and name [{index.CollectionName}] but it is already registered.");
        }

        registeredCollections.Add(index);
    }

    /// <summary>
    /// Resets all indicies
    /// </summary>
    /// <param name="models"></param>
    public void SetIndicies(IEnumerable<ITypesenseConfigurationModel> models)
    {
        registeredCollections.Clear();
        foreach (var index in models)
        {
            Instance.AddCollection(new TypesenseCollection(index, StrategyStorage.Strategies));
        }
    }

    /// <summary>
    /// Sets the current indicies to those provided by <paramref name="configurationService"/>
    /// </summary>
    /// <param name="configurationService"></param>
    public static void SetIndicies(ITypesenseConfigurationKenticoStorageService configurationService)
    {
        var indices = configurationService.GetAllCollectionData();

        Instance.SetIndicies(indices);
    }
}
