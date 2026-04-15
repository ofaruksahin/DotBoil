using System.Reflection;
using DotBoil.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace DotBoil.Dependency
{
    internal static class DependencyBootstrapper
    {
        public static async Task<IReadOnlyList<Module>> AddDotBoilDependencies()
        {
            var applicationConfiguration = DotBoilApp.Configuration.GetConfigurations<ApplicationConfiguration>();
            
            if (!applicationConfiguration.PreloadPrefix.Contains("DotBoil"))
                applicationConfiguration.PreloadPrefix.Add("DotBoil");
                
            foreach (var preloadPrefix in applicationConfiguration.PreloadPrefix)
                PreloadAssemblies(preloadPrefix);
            
            var modules = DiscoverModules();
            var sortedModules = SortModules(modules);

            foreach (var module in sortedModules)
                await module.AddModule();

            return sortedModules;
        }

        public static async Task UseDotBoilDependencies()
        {
            var modules = DiscoverModules();
            var sortedModules = SortModules(modules);
            
            foreach (var module in sortedModules)
                await module.UseModule();
        }

        private static List<Module> DiscoverModules()
        {
            var assemblies = DependencyContext.Default
                .GetDefaultAssemblyNames()
                .Select(Assembly.Load);
            
            var modules = assemblies
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch (ReflectionTypeLoadException e) { return e.Types.Where(t => t != null)!; }
                })
                .Where(t => typeof(Module).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .Select(t => (Module)Activator.CreateInstance(t))
                .ToList();

            return modules;
        }
        
        private static List<Module> SortModules(List<Module> modules)
        {
            var moduleMap = modules
                .DistinctBy(m => m.Name).ToDictionary(
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
        
        private static void PreloadAssemblies(string prefix)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
    
            foreach (var dll in Directory.GetFiles(baseDir, $"{prefix}.*.dll"))
            {
                var name = AssemblyName.GetAssemblyName(dll);
        
                if (AppDomain.CurrentDomain.GetAssemblies().All(a => a.FullName != name.FullName))
                    Assembly.Load(name);
            }
        }
    }
}
