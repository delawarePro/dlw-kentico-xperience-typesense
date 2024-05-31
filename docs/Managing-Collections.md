# Managing collections

To manage a search collection in the Xperience administration, navigate to the Search application on the dashboard.

![Administration dashboard highlight the Search application](/images/xperience-administration-dashboard.jpg)

Create a new collection or select and collection to edit by clicking the collection row or the pencil icon.

![Administration search collection list](/images/xperience-administration-search-collection-list.jpg)

Fill out the search collection form, populating the fields with your custom values.

![Administration search edit form](/images/xperience-administration-search-collection-edit-form.jpg)

- Rebuild Hook - for validating a request rebuild of the search collection from an external source (ex: API request)
- collectioned Languages - the collection will only include content in the selected languages
- Channel Name - the collection will only be triggered by web page item creation or modication in the selected website channel
- collectioning Strategy - the collectioning strategy specified in code during dependency registration of a custom collectioning strategies.
  - If you want the default strategy to appear here, register it explicitly in `IServiceCollection.AddKenticoAlgolia()` method

Now, configure the web page paths and content types that the search collection depends on by clicking the Add New Path button
or clicking an existing path in the table at the top of the collection configuration form.

![Administration search collection list](/images/xperience-administration-search-collection-edit-form-paths-edit.jpg)

- Included Path - can be an exact relative path of a web page item, (ex: `/path/to/my/page`), or a wildcard path (ex: `/parent-path/%`)
  - To determine a web page path, select the web page in the website channel page tree, then view the "Current URL" in the Content tab of the web page. The path will be the relative path excluding the domain
- Available content types - these are the web page content types that can be selected for the path.
- Included ContentType items - these are the web page content types that can be selected for the path. Each content type in the multi-select enables modification to web pages of that type to trigger an event that your custom collectioning strategy can process. If no option is selected, no web pages will trigger these events.

## collectioning reusable content items

All reusable content item modifications will trigger an event to generate a `collectionEventReusableItemModel` for your custom collection strategy class to process, as long as the content item has a language variant matching one of the languages selected for the collection. You can use this to collection reusable content items in addition to web page items but returning the reusable content item content as a `IcollectionEventItemModel` from the strategy `FindItemsToRecollection` method.

> Note: There currently no UI to allow administrators to configure which types of reusable content items trigger collectioning. This could be added in a future update.
