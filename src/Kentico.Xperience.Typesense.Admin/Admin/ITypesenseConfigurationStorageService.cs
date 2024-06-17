namespace Kentico.Xperience.Typesense.Admin;

public interface ITypesenseConfigurationKenticoStorageService
{
    Task<bool> TryCreateCollection(TypesenseConfigurationModel configuration);

    Task<bool> TryEditCollection(TypesenseConfigurationModel configuration);
    Task<bool> TryDeleteCollection(TypesenseConfigurationModel configuration);
    Task<bool> TryDeleteCollection(int collectionId);
    TypesenseConfigurationModel? GetCollectionDataOrNull(int collectionId);
    List<string> GetExistingcollectionNames();
    List<int> GetCollectionIds();
    IEnumerable<TypesenseConfigurationModel> GetAllCollectionData();
}
