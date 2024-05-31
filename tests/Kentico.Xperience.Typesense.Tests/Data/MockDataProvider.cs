using DancingGoat.Models;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;

namespace Kentico.Xperience.Typesense.Tests.Base;
internal static class MockDataProvider
{
    public static CollectionEventWebPageItemModel WebModel(CollectionEventWebPageItemModel item)
    {
        item.LanguageName = CzechLanguageName;
        item.ContentTypeName = ArticlePage.CONTENT_TYPE_NAME;
        item.Name = "Name";
        item.ContentTypeID = 1;
        item.ContentLanguageID = 1;
        item.WebsiteChannelName = DefaultChannel;
        item.WebPageItemTreePath = "/%";

        return item;
    }

    public static TypesenseCollectionIncludedPath Path => new("/%")
    {
        ContentTypes = [new TypesenseCollectionContentType(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))]
    };


    public static TypesenseCollection Collection => new(
        new TypesenseConfigurationModel()
        {
            CollectionName = DefaultCollection,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<TypesenseCollectionIncludedPath>() { Path },
            StrategyName = "strategy"
        },
        []
    );

    public static readonly string DefaultCollection = "SimpleCollection";
    public static readonly string DefaultChannel = "DefaultChannel";
    public static readonly string EnglishLanguageName = "en";
    public static readonly string CzechLanguageName = "cz";
    public static readonly int CollectionId = 1;
    public static readonly string EventName = "publish";

    public static TypesenseCollection GetCollection(string collectionName, int id) => new(
        new TypesenseConfigurationModel()
        {
            Id = id,
            CollectionName = collectionName,
            ChannelName = DefaultChannel,
            LanguageNames = new List<string>() { EnglishLanguageName, CzechLanguageName },
            Paths = new List<TypesenseCollectionIncludedPath>() { Path }
        },
        []
    );
}
