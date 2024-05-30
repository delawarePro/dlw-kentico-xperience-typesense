using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Collectioning;

/// <summary>
/// Represents a store of Typesense indexes and crawlers.
/// </summary>
public sealed class TypesenseCollectionStore
{
    private static readonly Lazy<TypesenseCollectionStore> mInstance = new();
    private readonly List<TypesenseCollection> registeredCollectiones = new();

    /// <summary>
    /// Gets current instance of the <see cref="TypesenseCollectionStore"/> class.
    /// </summary>
    public static TypesenseCollectionStore Instance => mInstance.Value;

    /// <summary>
    /// Gets all registered indexes.
    /// </summary>
    public IEnumerable<TypesenseCollection> GetAllIndices() => registeredCollectiones;

    /// <summary>
    /// Gets a registered <see cref="TypesenseCollection"/> with the specified <paramref name="collectionName"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="collectionName">The name of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public TypesenseCollection? GetCollection(string collectionName)
    {
        if (string.IsNullOrEmpty(collectionName))
        {
            return null;
        }

        return registeredCollectiones.SingleOrDefault(i => i.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a registered <see cref="TypesenseCollection"/> with the specified <paramref name="identifier"/>,
    /// or <c>null</c>.
    /// </summary>
    /// <param name="identifier">The identifier of the index to retrieve.</param>
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="InvalidOperationException" />
    public TypesenseCollection? GetCollection(int identifier) => registeredCollectiones.Find(i => i.Identifier == identifier);

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

        return registeredCollectiones.SingleOrDefault(i => i.CollectionName.Equals(collectionName, StringComparison.OrdinalIgnoreCase))
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

        if (registeredCollectiones.Exists(i => i.CollectionName.Equals(index.CollectionName, StringComparison.OrdinalIgnoreCase) || index.Identifier == i.Identifier))
        {
            throw new InvalidOperationException($"Attempted to register Typesense index with identifer [{index.Identifier}] and name [{index.CollectionName}] but it is already registered.");
        }

        registeredCollectiones.Add(index);
    }

    /// <summary>
    /// Resets all indicies
    /// </summary>
    /// <param name="models"></param>
    internal void SetIndicies(IEnumerable<TypesenseConfigurationModel> models)
    {
        registeredCollectiones.Clear();
        foreach (var index in models)
        {
            Instance.AddCollection(new TypesenseCollection(index, StrategyStorage.Strategies));
        }
    }

    /// <summary>
    /// Sets the current indicies to those provided by <paramref name="configurationService"/>
    /// </summary>
    /// <param name="configurationService"></param>
    internal static void SetIndicies(ITypesenseConfigurationStorageService configurationService)
    {
        var indices = configurationService.GetAllCollectionData();

        Instance.SetIndicies(indices);
    }
}
