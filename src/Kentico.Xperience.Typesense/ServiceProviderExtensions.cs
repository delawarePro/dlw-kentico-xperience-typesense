using Kentico.Xperience.Typesense.Collection;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceProviderExtensions
{
    /// <summary>
    /// Returns an instance of the <see cref="ITypesenseCollectionStrategy"/> assigned to the given <paramref name="index" />.
    /// Used to generate instances of a <see cref="ITypesenseCollectionStrategy"/> service type that can change at runtime.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="index"></param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the assigned <see cref="ITypesenseCollectionStrategy"/> cannot be instantiated.
    ///     This shouldn't normally occur because we fallback to <see cref="DefaultTypesenseCollectionStrategy" /> if not custom strategy is specified.
    ///     However, incorrect dependency management in user-code could cause issues.
    /// </exception>
    /// <returns></returns>
    public static ITypesenseCollectionStrategy GetRequiredStrategy(this IServiceProvider serviceProvider, TypesenseCollection index)
    {
        var strategy = serviceProvider.GetRequiredService(index.TypesenseCollectioningStrategyType) as ITypesenseCollectionStrategy;

        return strategy!;
    }
}
