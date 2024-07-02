using System.ComponentModel;

using Kentico.Xperience.Typesense.Admin.Admin;
using Kentico.Xperience.Typesense.Collection;
using Kentico.Xperience.Typesense.QueueWorker;
using Kentico.Xperience.Typesense.Xperience;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Typesense;

/// <summary>
/// Application startup extension methods.
/// </summary>
public static class TypesenseAdminStartupExtensions
{
    /// <summary>
    /// Adds Typesense services and custom module to application with customized options provided by the <see cref="ITypesenseBuilder"/>
    /// in the <paramref name="configure" /> action.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configure"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoAdminTypesense(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<TypesenseModuleInstaller>()
            .AddSingleton<ITypesenseConfigurationKenticoStorageService, DefaultTypesenseConfigurationKenticoStorageService>()
            .AddSingleton<ITypesenseCollectionService, DefaultTypesenseCollectionService>()
            .AddHostedService<TypesenseBackgroundWorker>();
}
