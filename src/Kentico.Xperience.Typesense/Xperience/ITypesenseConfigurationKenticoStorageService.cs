namespace Kentico.Xperience.Typesense.Xperience;

public interface ITypesenseConfigurationKenticoStorageService
{
    Task<bool> TryCreateCollection(ITypesenseConfigurationModel configuration);

    Task<bool> TryEditCollection(ITypesenseConfigurationModel configuration);
    Task<bool> TryDeleteCollection(ITypesenseConfigurationModel configuration);
    Task<bool> TryDeleteCollection(int collectionId);
    ITypesenseConfigurationModel? GetCollectionDataOrNull(int collectionId);
    List<string> GetExistingcollectionNames();
    List<int> GetCollectionIds();
    IEnumerable<ITypesenseConfigurationModel> GetAllCollectionData();
}
