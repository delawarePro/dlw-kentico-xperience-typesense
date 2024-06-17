using DancingGoat.Search.Services;

using Kentico.Xperience.Typesense;

namespace DancingGoat.Search;

public static class DancingGoatSearchStartupExtensions
{
    public static IServiceCollection AddKenticoTypesenseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKenticoTypesense(builder =>
        {
            builder.RegisterStrategy<AdvancedSearchCollectionStrategy>("DancingGoatAdvancedExampleStrategy");
            builder.RegisterStrategy<SimpleSearchCollectionStrategy>("DancingGoatMinimalExampleStrategy");
        }, configuration, false);

        services.AddHttpClient<WebCrawlerService>();
        services.AddSingleton<WebScraperHtmlSanitizer>();

        services.AddSingleton<SimpleSearchService>();
        services.AddSingleton<AdvancedSearchService>();

        return services;
    }
}

