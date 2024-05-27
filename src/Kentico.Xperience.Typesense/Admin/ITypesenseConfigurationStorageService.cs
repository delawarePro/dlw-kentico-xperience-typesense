namespace Kentico.Xperience.Typesense.Admin;

public interface ITypesenseConfigurationStorageService
{
    bool TryCreateCollection(TypesenseConfigurationModel configuration);

    bool TryEditCollection(TypesenseConfigurationModel configuration);
    bool TryDeleteCollection(TypesenseConfigurationModel configuration);
    bool TryDeleteCollection(int id);
    TypesenseConfigurationModel? GetCollectionDataOrNull(int indexId);
    List<string> GetExistingcollectionNames();
    List<int> GetCollectionIds();
    IEnumerable<TypesenseConfigurationModel> GetAllCollectionData();
}
