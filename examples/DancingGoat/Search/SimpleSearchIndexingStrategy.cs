using CMS.ContentEngine;
using CMS.Websites;
using DancingGoat.Models;
using Kentico.Xperience.Typesense.Collection;
using Microsoft.IdentityModel.Tokens;
using Kentico.Xperience.Typesense.Search;

namespace DancingGoat.Search;

public class SimpleSearchCollectionStrategy : DefaultTypesenseCollectionStrategy
{
    public class SimpleSearchResultModel : TypesenseSearchResultModel
    {
        public string Title { get; set; }
    }

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public SimpleSearchCollectionStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override async Task<IEnumerable<TypesenseSearchResultModel>?> MapToTypesenseObjectsOrNull(ICollectionEventItemModel typesensePageItem)
    {
        var result = new List<SimpleSearchResultModel>();

        // ICollectionEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (typesensePageItem is CollectionEventWebPageItemModel indexedPage)
        {
            if (string.Equals(typesensePageItem.ContentTypeName, HomePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                var page = await GetPage<HomePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    HomePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                if (page.HomePageBanner.IsNullOrEmpty())
                {
                    return null;
                }

                result.Add(new SimpleSearchResultModel()
                {
                    Title = page!.HomePageBanner.First().BannerHeaderText
                });


            }
            else if (string.Equals(typesensePageItem.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                var page = await GetPage<ArticlePage>(
                    indexedPage.ItemGuid,
                    indexedPage.WebsiteChannelName,
                    indexedPage.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                result.Add(new SimpleSearchResultModel()
                {
                    Title = page!.ArticleTitle
                });
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        return result;
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        var query = new ContentItemQueryBuilder()
            .ForContentType(contentTypeName,
                config =>
                    config
                        .WithLinkedItems(4) // You could parameterize this if you want to optimize specific database queries
                        .ForWebsite(channelName)
                        .Where(where => where.WhereEquals(nameof(WebPageFields.WebPageItemGUID), id))
                        .TopN(1))
            .InLanguage(languageName);

        var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<T>);

        return result.FirstOrDefault();
    }
}
