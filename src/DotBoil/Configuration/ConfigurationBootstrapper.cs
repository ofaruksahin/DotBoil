using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.Configuration
{
    internal static class ConfigurationBootstrapper
    {
        public static Task AddDotBoilConfigurations(params Assembly[] assemblies)
        {
            AddConfigurationProviders(assemblies);
            AddConfigurations(assemblies);

            return Task.CompletedTask;
        }

        private static void AddConfigurationProviders(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var configurationProviders = GetConfigurationProviders(assembly);

                foreach (var provider in configurationProviders)
                {
                    var providerSource = (IConfigurationSource)Activator.CreateInstance(provider);
                    (DotBoilApp.Configuration as ConfigurationManager).Sources.Add(providerSource);
                }
            }

            IReadOnlyList<Type> GetConfigurationProviders(Assembly assembly)
            {
                return assembly
                    .GetTypes()
                    .Where(type => type.GetInterface(nameof(IConfigurationSource)) is not null)
                    .ToList();
            }
        }

        private static void AddConfigurations(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var configurationLoaders = GetConfigurationLoaders(assembly);

                foreach (var configurationLoader in configurationLoaders)
                {
                    var configuration = (IOptions)Activator.CreateInstance(configurationLoader);

                    DotBoilApp.Configuration.GetSection(configuration.Key).Bind(configuration);
                    DotBoilApp.Services.AddSingleton(configurationLoader, configuration);
                }
            }

            IReadOnlyList<Type> GetConfigurationLoaders(Assembly assembly)
            {
                return assembly
                   .GetTypes()
                   .Where(type => type.GetInterface(nameof(IOptions)) is not null)
                   .ToList();
            }
        }
    }
}
