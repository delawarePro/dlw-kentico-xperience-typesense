namespace Kentico.Xperience.Typesense.Collection;
public interface ICollectionableModel
{
    public Guid ItemGuid { get; set; }
    public string ContentTypeName { get; set; }
}
