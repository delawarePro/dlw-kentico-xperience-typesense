﻿using CMS.ContentEngine;
using CMS.Websites;

namespace Kentico.Xperience.Typesense.Collection;

/// <summary>
/// Abstraction of different types of events generated from content modifications
/// </summary>
public interface ICollectionEventItemModel
{
    /// <summary>
    /// The identifier of the item
    /// </summary>
    int ItemID { get; set; }
    Guid ItemGuid { get; set; }
    string LanguageName { get; set; }
    string ContentTypeName { get; set; }
    string Name { get; set; }
    bool IsSecured { get; set; }
    int ContentTypeID { get; set; }
    int ContentLanguageID { get; set; }
}

public class CollectionEventWebPageItemModel : ICollectionEventItemModel
{
    /// <summary>
    /// The <see cref="WebPageFields.WebPageItemID"/> 
    /// </summary>
    public int ItemID { get; set; }
    /// <summary>
    /// The <see cref="WebPageFields.WebPageItemGUID"/>
    /// </summary>
    public Guid ItemGuid { get; set; }
    public string LanguageName { get; set; }
    public string ContentTypeName { get; set; }
    /// <summary>
    /// The <see cref="WebPageFields.WebPageItemName"/>
    /// </summary>
    public string Name { get; set; }
    public bool IsSecured { get; set; }
    public int ContentTypeID { get; set; }
    public int ContentLanguageID { get; set; }

    public string WebsiteChannelName { get; set; }
    public string WebPageItemTreePath { get; set; }
    public int? ParentID { get; set; }
    public int Order { get; set; }
    public string? Url { get; set; }

    public CollectionEventWebPageItemModel(
        int itemID,
        Guid itemGuid,
        string languageName,
        string contentTypeName,
        string name,
        bool isSecured,
        int contentTypeID,
        int contentLanguageID,
        string websiteChannelName,
        string webPageItemTreePath,
        int? parentID,
        int order,
        string? url
    )
    {
        ItemID = itemID;
        ItemGuid = itemGuid;
        LanguageName = languageName;
        ContentTypeName = contentTypeName;
        WebsiteChannelName = websiteChannelName;
        WebPageItemTreePath = webPageItemTreePath;
        ParentID = parentID;
        Order = order;
        Name = name;
        IsSecured = isSecured;
        ContentTypeID = contentTypeID;
        ContentLanguageID = contentLanguageID;
        Url = url;
    }

}

public class CollectionEventReusableItemModel : ICollectionEventItemModel
{
    /// <summary>
    /// The <see cref="ContentItemFields.ContentItemID"/>
    /// </summary>
    public int ItemID { get; set; }
    /// <summary>
    /// The <see cref="ContentItemFields.ContentItemGUID"/>
    /// </summary>
    public Guid ItemGuid { get; set; }
    public string LanguageName { get; set; }
    public string ContentTypeName { get; set; }
    /// <summary>
    /// The <see cref="ContentItemFields.ContentItemName"/>
    /// </summary>
    public string Name { get; set; }
    public bool IsSecured { get; set; }
    public int ContentTypeID { get; set; }
    public int ContentLanguageID { get; set; }

    public CollectionEventReusableItemModel(
        int itemID,
        Guid itemGuid,
        string languageName,
        string contentTypeName,
        string name,
        bool isSecured,
        int contentTypeID,
        int contentLanguageID
    )
    {
        ItemID = itemID;
        ItemGuid = itemGuid;
        LanguageName = languageName;
        ContentTypeName = contentTypeName;
        Name = name;
        IsSecured = isSecured;
        ContentTypeID = contentTypeID;
        ContentLanguageID = contentLanguageID;
    }
}

public class EndOfRebuildItemModel : ICollectionEventItemModel
{
    public int ItemID { get; set; }
    public Guid ItemGuid { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string ContentTypeName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSecured { get; set; }
    public int ContentTypeID { get; set; }
    public int ContentLanguageID { get; set; }

    public string CurrentlyActiveCollection { get; private set; }
    public string RebuildedCollection { get; private set; }
    public string CollectionAlias { get; private set; }

    public EndOfRebuildItemModel(string currentlyActiveCollection, string rebuildedCollection, string collectionAlias)
    {
        CurrentlyActiveCollection = currentlyActiveCollection;
        RebuildedCollection = rebuildedCollection;
        CollectionAlias = collectionAlias;
    }
}
