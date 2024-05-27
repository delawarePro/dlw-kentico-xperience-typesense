using CMS;
using CMS.Base;
using CMS.ContentEngine;
using CMS.Core;
using CMS.DataEngine;
using CMS.Websites;

using Kentico.Xperience.Typesense;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(TypesenseSearchModule))]

namespace Kentico.Xperience.Typesense;

/// <summary>
/// Initializes page event handlers, and ensures the thread queue workers for processing Typesense tasks.
/// </summary>
internal class TypesenseSearchModule : Module
{
    private ITypesenseTaskLogger? typesenseTaskLogger;
    private IAppSettingsService? appSettingsService;
    private IConversionService? conversionService;
    private const string APP_SETTINGS_KEY_INDEXING_DISABLED = "CMSTypesenseSearchDisableCollectioning";

    private bool CollectioningDisabled => conversionService?.GetBoolean(appSettingsService?[APP_SETTINGS_KEY_INDEXING_DISABLED], false) ?? false;

    /// <inheritdoc/>
    public TypesenseSearchModule() : base(nameof(TypesenseSearchModule))
    {
    }

    /// <inheritdoc/>
    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit();

        var services = parameters.Services;

        var options = services.GetRequiredService<IOptions<TypesenseOptions>>();

        if (!options.Value.IsConfigured)
        {
            return;
        }

        typesenseTaskLogger = services.GetRequiredService<ITypesenseTaskLogger>();
        appSettingsService = services.GetRequiredService<IAppSettingsService>();
        conversionService = services.GetRequiredService<IConversionService>();

        WebPageEvents.Publish.Execute += HandleEvent;
        WebPageEvents.Delete.Execute += HandleEvent;
        ContentItemEvents.Publish.Execute += HandleContentItemEvent;
        ContentItemEvents.Delete.Execute += HandleContentItemEvent;

        RequestEvents.RunEndRequestTasks.Execute += (sender, eventArgs) => TypesenseQueueWorker.Current.EnsureRunningThread();
    }


    /// <summary>
    /// Called when a page is published. Logs an Typesense task to be processed later.
    /// </summary>
    private void HandleEvent(object? sender, CMSEventArgs e)
    {
        if (CollectioningDisabled || e is not WebPageEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedItemModel = new CollectionEventWebPageItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID,
            publishedEvent.WebsiteChannelName,
            publishedEvent.TreePath,
            publishedEvent.ParentID,
            publishedEvent.Order,
            string.Empty) // TOOD : add the url)
        { };

        typesenseTaskLogger?.HandleEvent(indexedItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    private void HandleContentItemEvent(object? sender, CMSEventArgs e)
    {
        if (CollectioningDisabled || e is not ContentItemEventArgsBase publishedEvent)
        {
            return;
        }

        var indexedContentItemModel = new CollectionEventReusableItemModel(
            publishedEvent.ID,
            publishedEvent.Guid,
            publishedEvent.ContentLanguageName,
            publishedEvent.ContentTypeName,
            publishedEvent.Name,
            publishedEvent.IsSecured,
            publishedEvent.ContentTypeID,
            publishedEvent.ContentLanguageID
        );

        typesenseTaskLogger?.HandleReusableItemEvent(indexedContentItemModel, e.CurrentHandler.Name).GetAwaiter().GetResult();
    }

    public static void AddRegisteredIndices()
    {
        var options = Service.Resolve<IOptions<TypesenseOptions>>();

        if (!options.Value.IsConfigured)
        {
            return;
        }

        var configurationStorageService = Service.Resolve<ITypesenseConfigurationStorageService>();

        var indices = configurationStorageService.GetAllCollectionData();

        TypesenseCollectionStore.Instance.SetIndicies(indices);
    }
}
