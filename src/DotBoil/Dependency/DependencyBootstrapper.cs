using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Dependency
{
    internal static class DependencyBootstrapper
    {
        public static async Task<IReadOnlyList<Module>> AddDotBoilDependencies()
        {
            var modules = DiscoverModules();
            var sortedModules = SortModules(modules);

            foreach (var module in sortedModules)
                module.AddModule();

            return sortedModules;
        }

        public static async Task UseDotBoilDependencies()
        {
            var modules = DiscoverModules();
            var sortedModules = SortModules(modules);
            
            foreach (var module in sortedModules)
                module.UseModule();
        }

        private static List<Module> DiscoverModules()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t =>
                    typeof(Module).IsAssignableFrom(t) &&
                    !t.IsAbstract &&
                    !t.IsInterface)
                .Select(t => (Module)Activator.CreateInstance(t)!)
                .ToList();
        }
        
        private static List<Module> SortModules(List<Module> modules)
        {
            var moduleMap = modules.ToDictionary(
                m => m.Name,
                StringComparer.OrdinalIgnoreCase);

            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<Module>();

            void Visit(Module module)
            {
                if (visited.Contains(module.Name))
                    return;

                if (visiting.Contains(module.Name))
                    throw new InvalidOperationException(
                        $"Circular dependency detected at module '{module.Name}'");

                visiting.Add(module.Name);

                foreach (var dep in module.DependsOn)
                {
                    if (!moduleMap.TryGetValue(dep, out var depModule))
                        throw new InvalidOperationException(
                            $"Module '{module.Name}' depends on '{dep}' but it was not found");

                    Visit(depModule);
                }

                visiting.Remove(module.Name);
                visited.Add(module.Name);
                result.Add(module);
            }

            foreach (var module in modules)
                Visit(module);

            return result
                .OrderBy(m => m.Order)
                .ToList();
        }
    }
}
