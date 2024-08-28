using System.Text.Json;

using Kentico.Xperience.Typesense.Collection;
using Kentico.Xperience.Typesense.JsonResolvers;
using Kentico.Xperience.Typesense.QueueWorker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Typesense.Setup;

namespace Kentico.Xperience.Typesense;

/// <summary>
/// Application startup extension methods.
/// </summary>
public static class TypesenseStartupExtensions
{
    /// <summary>
    /// Registers instances of <see cref="IInsightsClient"/> and <see cref="IXperienceTypesenseClient"/> with
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    private static IServiceCollection AddKenticoTypesenseInternal(this IServiceCollection services, IConfiguration configuration)
    {
        var typesenseSection = configuration.GetSection(TypesenseOptions.CMS_TYPESENSE_SECTION_NAME);
        var typesenseOptions = typesenseSection.GetChildren();

        bool isConfigured = false;

        var nodeOptions = typesenseOptions.Single(x => x.Key == nameof(TypesenseOptions.Node));

        if (typesenseOptions.Single(x => x.Key == nameof(TypesenseOptions.ApiKey)).Value != ""
            && nodeOptions != null
            && nodeOptions.GetChildren().Single(x => x.Key == nameof(NodeOptions.Host)).Value != ""
            && nodeOptions.GetChildren().Single(x => x.Key == nameof(NodeOptions.Protocol)).Value != ""
            && nodeOptions.GetChildren().Single(x => x.Key == nameof(NodeOptions.Port)).Value != "")
        {
            isConfigured = true;
        }
        else
        {
            throw new ArgumentException("Typesense configuration is not valid. Please check the configuration in the appsettings.json file.");
        }

        services
            .Configure<TypesenseOptions>(typesenseSection)
            .PostConfigure<TypesenseOptions>(options => options.IsConfigured = isConfigured);

        // Get an object of type IOptions<TypesenseOptions>
        var options = services.BuildServiceProvider().GetRequiredService<IOptions<TypesenseOptions>>();

        if (!isConfigured)
        {
            return services;
        }


#pragma warning disable format
        return services
            .AddTypesenseClient(config =>
            {

                config.ApiKey = options.Value.ApiKey;
                config.Nodes = new List<Node> { new( options.Value.Node.Host, options.Value.Node.Port.ToString(), options.Value.Node.Protocol) };
                config.JsonSerializerOptions = new JsonSerializerOptions()
                {
                    TypeInfoResolver = new TypeSenseTypeResolver()
                };
            })
            /*
            .AddSingleton<IXperienceTypesenseClient>(s =>
            {
                var options = s.GetRequiredService<IOptions<TypesenseOptions>>();
                var configuration = new SearchConfig(options.Value.ApplicationId, options.Value.ApiKey);
                var fileVersion = FileVersionInfo.GetVersionInfo(typeof(TypesenseOptions).Assembly.Location);
                string versioNumber = new Version(fileVersion.FileVersion ?? "").ToString(3);
                configuration.DefaultHeaders["User-Agent"] = $"Kentico Xperience for Typesense ({versioNumber})";

                return new SearchClient(configuration);
            })*/
           .AddSingleton<IXperienceTypesenseClient, DefaultTypesenseClient>()
           .AddSingleton<ITypesenseTaskLogger, DefaultTypesenseTaskLogger>()
           .AddSingleton<ITypesenseTaskProcessor, DefaultTypesenseTaskProcessor>();
#pragma warning restore format
    }
    /// <summary>
    /// Adds Typesense services and custom module to application with customized options provided by the <see cref="ITypesenseBuilder"/>
    /// in the <paramref name="configure" /> action.
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configure"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="useInMemoryQueue">If true, the <see cref="InMemoryQueue"/> will be used for indexing tasks. Otherwise, the <see cref="SqlQueue"/> will be used.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoTypesense(this IServiceCollection serviceCollection, Action<ITypesenseBuilder> configure, IConfiguration configuration, bool useInMemoryQueue)
    {
        serviceCollection.AddKenticoTypesenseInternal(configuration);
        if (useInMemoryQueue)
        {
            serviceCollection.AddSingleton<ITypesenseQueue, InMemoryQueue>();
        }
        else
        {
            serviceCollection.AddSingleton<ITypesenseQueue, SqlQueue>();
        }

        var builder = new TypesenseBuilder(serviceCollection);

        configure(builder);

        if (builder.IncludeDefaultStrategy)
        {
            serviceCollection.AddTransient<DefaultTypesenseCollectionStrategy>();
            builder.RegisterStrategy<DefaultTypesenseCollectionStrategy>("Default");
        }

        return serviceCollection;
    }

    /// <summary>
    /// Adds Typesense services and custom module to application with <see cref="DefaultTypesenseCollectionStrategy"/>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddKenticoTypesense(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddKenticoTypesenseInternal(configuration);

        var builder = new TypesenseBuilder(serviceCollection);

        serviceCollection.AddTransient<DefaultTypesenseCollectionStrategy>();
        builder.RegisterStrategy<DefaultTypesenseCollectionStrategy>("Default");

        return serviceCollection;
    }
}


public interface ITypesenseBuilder
{
    /// <summary>
    /// Registers the given <typeparamref name="TStrategy" /> as a transient service under <paramref name="strategyName" />
    /// </summary>
    /// <typeparam name="TStrategy">The custom type of <see cref="ITypesenseCollectionStrategy"/> </typeparam>
    /// <param name="strategyName">Used internally <typeparamref name="TStrategy" /> to enable dynamic assignment of strategies to search indexes. Names must be unique.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if an strategy has already been registered with the given <paramref name="strategyName"/>
    /// </exception>
    /// <returns></returns>
    ITypesenseBuilder RegisterStrategy<TStrategy>(string strategyName) where TStrategy : class, ITypesenseCollectionStrategy;
}

internal class TypesenseBuilder : ITypesenseBuilder
{
    private readonly IServiceCollection serviceCollection;

    /// <summary>
    /// If true, the <see cref="DefaultTypesenseCollectionStrategy" /> will be available as an explicitly selectable indexing strategy
    /// within the Admin UI. Defaults to <c>true</c>
    /// </summary>
    public bool IncludeDefaultStrategy { get; set; } = true;

    public TypesenseBuilder(IServiceCollection serviceCollection) => this.serviceCollection = serviceCollection;

    public ITypesenseBuilder RegisterStrategy<TStrategy>(string strategyName) where TStrategy : class, ITypesenseCollectionStrategy
    {
        StrategyStorage.AddStrategy<TStrategy>(strategyName);
        serviceCollection.AddSingleton<TStrategy>();

        return this;
    }
}
