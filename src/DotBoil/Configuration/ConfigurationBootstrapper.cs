using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.Configuration
{
    internal static class ConfigurationBootstrapper
    {
        public static Task AddDotBoilConfigurations()
        {
            AddConfigurationProviders();
            AddConfigurations();
            AddConfigurationProviders();

            return Task.CompletedTask;
        }

        private static void AddConfigurationProviders()
        {
            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(ass => ass.FullName.Contains("DotBoil"))
                .ToList();
            
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

        private static void AddConfigurations()
        {
            var assemblies = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .Where(ass => ass.FullName.Contains("DotBoil"))
                .ToList();
            
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
