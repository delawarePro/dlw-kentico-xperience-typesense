﻿using CMS;
using CMS.Base;
using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Typesense.Collection;
using Kentico.Xperience.Typesense.Xperience;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: RegisterModule(typeof(TypesenseAdminModule))]

namespace Kentico.Xperience.Typesense.Xperience;

/// <summary>
/// Manages administration features and integration.
/// </summary>
internal class TypesenseAdminModule : AdminModule
{
    private ITypesenseConfigurationKenticoStorageService storageService = null!;
    private TypesenseModuleInstaller installer = null!;

    public TypesenseAdminModule() : base(nameof(TypesenseAdminModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        var options = services.GetRequiredService<IOptions<TypesenseOptions>>();

        if (!options.Value.IsConfigured)
        {
            return;
        }

        RegisterClientModule("delaware", "xperience-integrations-typesense");

        installer = services.GetRequiredService<TypesenseModuleInstaller>();
        storageService = services.GetRequiredService<ITypesenseConfigurationKenticoStorageService>();
        installer.Install();
        ApplicationEvents.PostStart.Execute += InitializeModule;
    }

    protected override void OnPreInit(ModulePreInitParameters parameters)
    {
        PreInit(parameters);
        var services = parameters.Services;
        services.AddKenticoAdminTypesense();
    }

    private void InitializeModule(object? sender, EventArgs e) => TypesenseCollectionStore.SetIndicies(storageService);
}
