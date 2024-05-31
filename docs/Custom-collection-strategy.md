# Create a custom collection strategy

The primary functionality of this library is enabled through a custom "Collection strategy" which is entirely based on your
content model and search experience. Below we will look at the steps and features available to define this Collection process.

## Implement an collection strategy type

Define a custom `DefaultTypesenseCollectionStrategy` implementation to customize how page or content items are processed for Collection.

Your custom implemention of `DefaultTypesenseCollectionStrategy` can use dependency injection to define services and configuration used for gathering the content to be collectioned. `DefaultTypesenseCollectionStrategy` implements `ITypesenseCollectionStrategy` and will be [registered as a transient](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#transient) in the DI container.

## Specify a mapping process

Override the `collectionSettings GetTypesensecollectionSettings()` method to specify saved attributes and their functionality. See Typesense documentation for this.

Override the `Task<IEnumerable<JObject>?> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel item)` method and define a process for mapping custom fields of each content item event provided.

The method is given an `IcollectionEventItemModel` which is a abstraction of any item being processed for Collection, which includes both `collectionEventWebPageItemModel` for web page items and `collectionEventReusableItemModel` for reusable content items. Every item specified in the admin ui is rebuilt. In the UI you need to specify one or more language, channel name, CollectionStrategy and paths with content types. This strategy than evaluates all web page items specified in the administration.

Let's say we specified `ArticlePage` in the admin ui.
Now we implement how we want to save ArticlePage page in our strategy.

The JObject is collectioned representation of the webpageitem.

You specify what fields should be collectioned in the JObject by adding them to the `collectionSettings`. You later retrieve data from the JObject based on your implementation.

```csharp
public class ExampleSearchCollectionStrategy : DefaultTypesenseCollectionStrategy
{
    public static string SORTABLE_TITLE_FIELD_NAME = "SortableTitle";

    public override collectionSettings GetTypesensecollectionSettings() =>
        new()
        {
            AttributesToRetrieve = new List<string>
            {
                nameof(DancingGoatSimpleSearchResultModel.Title)
            }
        };

    public override async Task<IEnumerable<JObject>> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel TypesensePageItem)
    {
        var result = new List<JObject>();

        string title = "";

        // IcollectionEventItemModel could be a reusable content item or a web page item, so we use
        // pattern matching to get access to the web page item specific type and fields
        if (TypesensePageItem is collectionEventWebPageItemModel collectionedPage)
        {
            if (string.Equals(TypesensePageItem.ContentTypeName, HomePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
            {
                var page = await GetPage<HomePage>(
                    collectionedPage.ItemGuid,
                    collectionedPage.WebsiteChannelName,
                    collectionedPage.LanguageName,
                    HomePage.CONTENT_TYPE_NAME);

                if (page is null)
                {
                    return null;
                }

                if (page.HomePageBanner.IsNullOrEmpty())
                {
                    return null;
                }

                title = page!.HomePageBanner.First().BannerHeaderText;
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

        var jObject = new JObject();
        jObject[nameof(DancingGoatSimpleSearchResultModel.Title)] = title;

        result.Add(jObject);

        return result;
    }
}
```

Some properties of the `IcollectionEventItemModel` are added to the JObjects by default by the library and these can be found in the `TypesenseSearchResultModel` class.

```csharp
// BaseJObjectProperties.cs

public class TypesenseSearchResultModel
{
    public string Url { get; set; } = "";
    public string ContentTypeName { get; set; } = "";
    public string LanguageName { get; set; } = "";
    public Guid ItemGuid { get; set; }
    public string ObjectID { get; set; } = "";
}
```

Override the class and use the name of these properties to specify the `collectionSettings` and later use this class to retrieve these data from collectioned Object.

```csharp
public class DancingGoatSearchResultModel : TypesenseSearchResultModel
{
    public string Title { get; set; }
    public string SortableTitle { get; set; }
    public string Content { get; set; }
}
```

```csharp
public class ExampleSearchCollectionStrategy : DefaultTypesenseCollectionStrategy
{
    // ...

    public override collectionSettings GetTypesensecollectionSettings() => new()
    {
        AttributesToRetrieve = new List<string>
            {
                nameof(DancingGoatSearchResultModel.Title),
                nameof(DancingGoatSearchResultModel.SortableTitle),
                nameof(DancingGoatSearchResultModel.Content)
            },
        AttributesForFaceting = new List<string>
            {
                nameof(DancingGoatSearchResultModel.ContentTypeName)
            }
    };

    public override async Task<IEnumerable<JObject>> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel TypesensePageItem)
    {
        var resultProperties = new DancingGoatSearchResultModel();

        // ...

        var result = new List<JObject>()
        {
            AssignProperties(resultProperties)
        };

        return result;
    }

    private JObject AssignProperties<T>(T value) where T : TypesenseSearchResultModel
    {
        var jObject = new JObject();

        foreach (var prop in value.GetType().GetProperties())
        {
            var type = prop.PropertyType;
            if (type == typeof(string))
            {
                jObject[prop.Name] = (string)prop.GetValue(value);
            }
            else if (type == typeof(int))
            {
                jObject[prop.Name] = (int)prop.GetValue(value);
            }
            else if (type == typeof(bool))
            {
                jObject[prop.Name] = (bool)prop.GetValue(value);
            }
        }

        return jObject;
    }
}
```

The `Url` field is a relative path by default. You can change this by adding this field manually in the `MapToTypesenseJObjectsOrNull` method.

```csharp
public override async Task<IEnumerable<JObject>?> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel item)
{
    //...

    var result = new List<JObject>();

    // retrieve an absolute URL
    if (item is collectionEventWebPageItemModel webpageItem &&
        string.Equals(collectionedModel.ContentTypeName, ArticlePage.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
    {
        string url = string.Empty;
        try
        {
            url = (await urlRetriever.Retrieve(
                webpageItem.WebPageItemTreePath,
                webpageItem.WebsiteChannelName,
                webpageItem.LanguageName)).AbsolutePath;
        }
        catch (Exception)
        {
            // Retrieve can throw an exception when processing a page update TypesenseQueueItem
            // and the page was deleted before the update task has processed. In this case, upsert an
            // empty URL
        }

        var jObject = new JObject();
            jObject[nameof(TypesenseSearchResultModel.Url)] = url;
    }

    //...
}
```

## Data retrieval during Collection

It is up to your implementation how do you want to retrieve the content or data to be collectioned, however any web page item could be retrieved using a generic `GetPage<T>` method. In the example below, you specify that you want to retrieve `ArticlePage` item in the provided language on the channel using provided id and content type.

```csharp
public class ExampleSearchCollectionStrategy : DefaultTypesenseCollectionStrategy
{
    // Other fields defined in previous examples
    // ...

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchCollectionStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override collectionSettings GetTypesensecollectionSettings()
    {
        // Same as examples above
        // ...
    }

    public override async Task<IEnumerable<JObject>?> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel TypesensePageItem)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
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

    private JObject AssignProperties<T>(T value) where T : TypesenseSearchResultModel
    {
        // Same as examples above
        // ...
    }
}
```

## Keeping collectioned related content up to date

If an collectioned web page item has relationships to other web page items or reusable content items, and updates to those items should trigger
a recollection of the original web page item, you can override the `Task<IEnumerable<IcollectionEventItemModel>> FindItemsToRecollection(collectionEventWebPageItemModel changedItem)` or `Task<IEnumerable<IcollectionEventItemModel>> FindItemsToRecollection(collectionEventReusableItemModel changedItem)` methods which both return the items that should be collectioned based on the incoming item being changed.

In our example an `ArticlePage` web page item has a `ArticlePageArticle` field which represents a reference to related reusable content items that contain the full article content. We include content from the reusable item in our collectioned web page, so changes to the reusable item should result in the collection being updated for the web page item.

All items returned from either `FindItemsToRecollection` method will be passed to `Task<IEnumerable<JObject>> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel TypesensePageItem)` for Collection.

```csharp
public class ExampleSearchCollectionStrategy : DefaultTypesenseCollectionStrategy
{
    // Other fields defined in previous examples
    // ...

    public const string collectionED_WEBSITECHANNEL_NAME = "mywebsitechannel";

    private readonly IWebPageQueryResultMapper webPageMapper;
    private readonly IContentQueryExecutor queryExecutor;

    public ExampleSearchCollectionStrategy(
        IWebPageQueryResultMapper webPageMapper,
        IContentQueryExecutor queryExecutor,
    )
    {
        this.webPageMapper = webPageMapper;
        this.queryExecutor = queryExecutor;
    }

    public override collectionSettings GetTypesensecollectionSettings()
    {
        // Same as examples above
        // ...
    }

    public override async Task<IEnumerable<JObject>> MapToTypesenseJObjectsOrNull(IcollectionEventItemModel TypesensePageItem)
    {
        // Implementation detailed in previous examples, including GetPage<T> call
        // ...
    }

    public override async Task<IEnumerable<IcollectionEventItemModel>> FindItemsToRecollection(collectionEventReusableItemModel changedItem)
    {
        var recollectionedItems = new List<IcollectionEventItemModel>();

        if (string.Equals(collectionedModel.ContentTypeName, Article.CONTENT_TYPE_NAME, StringComparison.OrdinalIgnorecase))
        {
            var query = new ContentItemQueryBuilder()
                .ForContentType(ArticlePage.CONTENT_TYPE_NAME,
                    config =>
                        config
                            .WithLinkedItems(4)

                            // Because the changedItem is a reusable content item, we don't have a website channel name to use here
                            // so we use a hardcoded channel name.
                            //
                            // This will be resolved with an upcoming Xperience by Kentico feature
                            // https://roadmap.kentico.com/c/193-new-api-cross-content-type-querying
                            .ForWebsite(collectionED_WEBSITECHANNEL_NAME)

                            // Retrieves all ArticlePages that link to the Article through the ArticlePage.ArticlePageArticle field
                            .Linking(nameof(ArticlePage.ArticlePageArticle), new[] { changedItem.ItemID }))
                .InLanguage(changedItem.LanguageName);

            var result = await queryExecutor.GetWebPageResult(query, webPageMapper.Map<ArticlePage>);

            foreach (var articlePage in result)
            {
                // This will be a IcollectionEventItemModel passed to our MapToTypesenseJObjectsOrNull method above
                recollectionable.Add(new collectionEventWebPageItemModel(
                    page.SystemFields.WebPageItemID,
                    page.SystemFields.WebPageItemGUID,
                    changedItem.LanguageName,
                    ArticlePage.CONTENT_TYPE_NAME,
                    page.SystemFields.WebPageItemName,
                    page.SystemFields.ContentItemIsSecured,
                    page.SystemFields.ContentItemContentTypeID,
                    page.SystemFields.ContentItemCommonDataContentLanguageID,
                    collectionED_WEBSITECHANNEL_NAME,
                    page.SystemFields.WebPageItemTreePath,
                    page.SystemFields.WebPageItemParentID,
                    page.SystemFields.WebPageItemOrder));
            }
        }

        return recollectionedItems;
    }

    private async Task<T?> GetPage<T>(Guid id, string channelName, string languageName, string contentTypeName)
        where T : IWebPageFieldsSource, new()
    {
        // Same as examples above
        // ...
    }

    private JObject AssignProperties<T>(T value) where T : TypesenseSearchResultModel
    {
        // Same as examples above
        // ...
    }
}
```

Note that we are not preparing the Typesense `JObject` in `FindItemsToRecollection`, but instead are generating a collection of
additional items that will need reCollection based on the modification of a related `IcollectionEventItemModel`.

## Collection web page content

See [Scraping web page content](Scraping-web-page-content.md)

## DI Registration

Finally, add this library to the application services, registering your custom `DefaultTypesenseCollectionStrategy` and Typesense

```csharp
// Program.cs

// Registers all services and uses default Collection behavior (no custom data will be collectioned)
services.AddKenticoTypesense();

// or

// Registers all services and enables custom Collection behavior
services.AddKenticoTypesense(builder =>
    builder
        .RegisterStrategy<ExampleSearchCollectionStrategy>("ExampleStrategy")
        ,configuration);
```
