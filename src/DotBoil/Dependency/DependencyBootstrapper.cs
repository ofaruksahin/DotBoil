using System.Reflection;

namespace DotBoil.Dependency
{
    internal static class DependencyBootstrapper
    {
        public static async Task AddDotBoilDependencies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var moduleLoaders = GetDependencyLoaders(assembly);

                foreach (var moduleLoader in moduleLoaders)
                {
                    var module = (Module)Activator.CreateInstance(moduleLoader);
                    await module.AddModule();
                }
            }
        }

        public static async Task UseDotBoilDependencies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var moduleLoaders = GetDependencyLoaders(assembly);

                foreach (var moduleLoader in moduleLoaders)
                {
                    var module = (Module)Activator.CreateInstance(moduleLoader);
                    await module.UseModule();
                }
            }
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
