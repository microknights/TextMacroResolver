using System;
using System.Linq;
using MicroKnights.Texting.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroKnights.Texting.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddTextMacroResolver<TMacroValueContext>(this IServiceCollection serviceCollection, Action<TextMacroResolverOptions> configureOptions) where TMacroValueContext : class
        {
            return AddTextMacroResolver(serviceCollection, configureOptions, typeof(TMacroValueContext));
        }

        public static IServiceCollection AddTextMacroResolver(this IServiceCollection serviceCollection, Action<TextMacroResolverOptions> configureOptions, Type macroValueContextType = null)
        {
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            var options = new TextMacroResolverOptions();

            serviceCollection.AddSingleton(options);
            serviceCollection.AddScoped<TextMacroResolver>();
            if(macroValueContextType != null)
                serviceCollection.TryAddScoped(macroValueContextType);
            configureOptions(options);
            foreach (var resolverType in options.GetMacroValueResolverTypes())
            {
                serviceCollection.TryAddScoped(resolverType.Value.Type);
            }
            return serviceCollection;
        }
    }
}