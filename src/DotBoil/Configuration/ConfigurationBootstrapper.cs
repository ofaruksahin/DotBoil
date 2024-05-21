using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.Configuration
{
    internal static class ConfigurationBootstrapper
    {
        public static Task<WebApplicationBuilder> AddDotBoilConfigurations(this WebApplicationBuilder builder, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var configurationLoaders = GetConfigurationLoaders(assembly);

                foreach (var configurationLoader in configurationLoaders)
                {
                    var configuration = (IOptions)Activator.CreateInstance(configurationLoader);

                    builder.Configuration.GetSection(configuration.Key).Bind(configuration);
                    builder.Services.AddSingleton(configurationLoader, configuration);
                }
            }

            return Task.FromResult(builder);
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
