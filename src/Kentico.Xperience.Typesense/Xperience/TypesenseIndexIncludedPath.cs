using System.Text.Json.Serialization;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseContentTypeItem;
using Kentico.Xperience.Typesense.Xperience.InfoModels.TypesenseIncludedPathItem;

namespace Kentico.Xperience.Typesense.Xperience;

public class TypesenseCollectionIncludedPath
{
    /// <summary>
    /// The node alias pattern that will be used to match pages in the content tree for indexing.
    /// </summary>
    /// <remarks>For example, "/Blogs/Products/" will index all pages under the "Products" page.</remarks>
    public string AliasPath { get; }

    /// <summary>
    /// A list of content types under the specified <see cref="AliasPath"/> that will be indexed.
    /// </summary>
    public List<TypesenseCollectionContentType> ContentTypes { get; set; } = [];

    /// <summary>
    /// The internal identifier of the included path.
    /// </summary>
    public string? Identifier { get; set; }

    [JsonConstructor]
    public TypesenseCollectionIncludedPath(string aliasPath) => AliasPath = aliasPath;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indexPath"></param>
    /// <param name="contentTypes"></param>
    /// <param name="typesenseContentTypeItemInfos"></param>
    /// <param name="contentTypeLinked"></param>
    public TypesenseCollectionIncludedPath(TypesenseIncludedPathItemInfo indexPath,
        IEnumerable<TypesenseCollectionContentType> contentTypes,
        IEnumerable<TypesenseContentTypeItemInfo> typesenseContentTypeItemInfos)
    {
        var contentTypesToLink = typesenseContentTypeItemInfos.Where(x =>
            x.TypesenseContentTypeItemIncludedPathItemId == indexPath.TypesenseIncludedPathItemId);
        var linkedContentType = contentTypes.Where(x => contentTypesToLink.Select(ctl => ctl.TypesenseContentTypeItemContentTypeName).Contains(x.ContentTypeName));
        AliasPath = indexPath.TypesenseIncludedPathItemAliasPath;
        ContentTypes = linkedContentType.ToList();
        Identifier = indexPath.TypesenseIncludedPathItemId.ToString();
    }
}
