
using Kentico.Xperience.Typesense.Xperience;

namespace Kentico.Xperience.Typesense.Collection;
public interface ITypesenseCollectionService
{
    public Task<bool> CreateOrEditCollection(ITypesenseConfigurationModel configuration);

}
