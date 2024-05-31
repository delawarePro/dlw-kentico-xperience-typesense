namespace Kentico.Xperience.Typesense.Admin;

public interface ITypesenseConfigurationTypesenseStorageService
{
    Task<bool> TryCreateCollection(TypesenseConfigurationModel configuration);

    Task<bool> TryEditCollection(TypesenseConfigurationModel configuration);
    Task<bool> TryDeleteCollection(TypesenseConfigurationModel? configuration);
}
