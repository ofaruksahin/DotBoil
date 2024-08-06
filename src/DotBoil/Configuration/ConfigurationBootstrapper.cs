using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.Configuration
{
    internal static class ConfigurationBootstrapper
    {
        public static Task AddDotBoilConfigurations(params Assembly[] assemblies)
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

            return Task.CompletedTask;
        }

        private static IReadOnlyList<Type> GetConfigurationLoaders(Assembly assembly)
        {
            return assembly
                    .GetTypes()
                    .Where(type => type.GetInterface(nameof(IOptions)) is not null)
                    .ToList();
        }
    }
}
