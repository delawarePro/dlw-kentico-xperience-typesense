using Kentico.Xperience.Typesense.Admin;

namespace Kentico.Xperience.Typesense.Collection;
public interface ITypesenseCollectionService
{
    Task<bool> CreateOrEditCollection(TypesenseConfigurationModel configuration);

}
