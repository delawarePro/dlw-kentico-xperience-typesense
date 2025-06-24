using Kentico.Xperience.Typesense.Admin;
using Kentico.Xperience.Typesense.Collection;
using Kentico.Xperience.Typesense.Query;
using Kentico.Xperience.Typesense.QueueWorker;
using Kentico.Xperience.Typesense.Xperience;

using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Xperience.Typesense;

/// <summary>
/// Application startup extension methods.
/// </summary>
public static class TypesenseAdminStartupExtensions
{    /// <summary>
     /// Adds Typesense services and custom module to application.
     /// </summary>
     /// <param name="serviceCollection"></param>
     /// <returns></returns>
    public static IServiceCollection AddKenticoAdminTypesense(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddSingleton<TypesenseModuleInstaller>()
            .AddSingleton<ITypesenseConfigurationKenticoStorageService, DefaultTypesenseConfigurationKenticoStorageService>()
            .AddSingleton<ITypesenseCollectionService, DefaultTypesenseCollectionService>()
            .AddSingleton<ITypesenseQueryService, DefaultTypesenseQueryService>()
            .AddHostedService<TypesenseBackgroundWorker>();
}
