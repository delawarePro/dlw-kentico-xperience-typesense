namespace Kentico.Xperience.Typesense.Admin;

public class TypesenseCollectionContentType
{
    /// <summary>
    /// Name of the indexed content type for an indexed path
    /// </summary>
    public string ContentTypeName { get; set; } = "";

    /// <summary>
    /// Displayed name of the indexed content type for an indexed path which will be shown in admin UI
    /// </summary>
    public string ContentTypeDisplayName { get; set; } = "";

    public TypesenseCollectionContentType()
    { }

    public TypesenseCollectionContentType(string className, string classDisplayName)
    {
        ContentTypeName = className;
        ContentTypeDisplayName = classDisplayName;
    }
}
