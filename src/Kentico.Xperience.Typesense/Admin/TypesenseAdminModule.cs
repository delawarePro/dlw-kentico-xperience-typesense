using CMS;
using CMS.Base;
using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collectioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(TypesenseAdminModule))]

namespace Kentico.Xperience.Typesense.Admin;

/// <summary>
/// Manages administration features and integration.
/// </summary>
internal class TypesenseAdminModule : AdminModule
{
    private ITypesenseConfigurationStorageService storageService = null!;
    private TypesenseModuleInstaller installer = null!;

    public TypesenseAdminModule() : base(nameof(TypesenseAdminModule)) { }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        var options = services.GetRequiredService<IOptions<TypesenseOptions>>();

        if (!options.Value.IsConfigured)
        {
            return;
        }

        RegisterClientModule("kentico", "xperience-integrations-typesense");

        installer = services.GetRequiredService<TypesenseModuleInstaller>();
        storageService = services.GetRequiredService<ITypesenseConfigurationStorageService>();

        ApplicationEvents.PostStart.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e)
    {
        installer.Install();

        TypesenseCollectionStore.SetIndicies(storageService);
    }
}
