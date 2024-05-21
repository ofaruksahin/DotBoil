using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace DotBoil.Dependency
{
    internal static class DependencyBootstrapper
    {
        public static async Task<WebApplicationBuilder> AddDotBoilDependencies(this WebApplicationBuilder builder, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var moduleLoaders = GetDependencyLoaders(assembly);

                foreach (var moduleLoader in moduleLoaders)
                {
                    var module = (Module)Activator.CreateInstance(moduleLoader);
                    await module.AddModule(builder);
                }
            }

            return builder;
        }

        public static async Task<WebApplication> UseDotBoilDependencies(this WebApplication app, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var moduleLoaders = GetDependencyLoaders(assembly);

                foreach (var moduleLoader in moduleLoaders)
                {
                    var module = (Module)Activator.CreateInstance(moduleLoader);
                    await module.UseModule(app);
                }
            }
            return app;
        }

        private static IReadOnlyList<Type> GetDependencyLoaders(Assembly assembly)
        {
            return assembly
                    .GetTypes()
                    .Where(type => type.BaseType == typeof(Module))
                    .ToList();
        }
    }
}
