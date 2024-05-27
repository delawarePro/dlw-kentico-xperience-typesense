using CMS.ContentEngine;

namespace Kentico.Xperience.Typesense.Search;

public class TypesenseSearchResultModel
{
    public Guid ItemGuid { get; set; }
    public string ContentTypeName { get; set; } = "";
    public string LanguageName { get; set; } = "";
    public string ObjectID { get; set; } = "";
    public string Url { get; set; } = "";

    public TypesenseSearchResultModel()
    {
    }

    public TypesenseSearchResultModel(IContentItemFieldsSource contentItemFieldsSource)
    {
        ItemGuid = contentItemFieldsSource.SystemFields.ContentItemGUID;
        ObjectID = contentItemFieldsSource.SystemFields.ContentItemID.ToString();

        //TODO : ContentTypename
        //TODO : LanguageName
        //TODO : Url

    }
}
