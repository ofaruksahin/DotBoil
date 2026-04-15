using DotBoil.Configuration;
using DotBoil.TemplateEngine.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorLight;
using System.Reflection;
using Module = DotBoil.Dependency.Module;

namespace DotBoil.TemplateEngine
{
    internal class TemplateEngineModule : Module
    {
        public override string Name => "TemplateEngine";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 15;

        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<RazorViewEngineConfiguration>();

            var entryAssembly = Assembly.GetEntryAssembly();
            var candidateAssemblies = new List<Assembly>();

            if (entryAssembly != null)
            {
                candidateAssemblies.Add(entryAssembly);
                var referenced = entryAssembly
                    .GetReferencedAssemblies()
                    .Where(a => !a.Name.StartsWith("Microsoft.Build"))
                    .Select(a => { try { return Assembly.Load(a); } catch { return null; } })
                    .Where(a => a != null);
                candidateAssemblies.AddRange(referenced);
            }

            var assembly = candidateAssemblies
                .FirstOrDefault(ass => ass.GetName().Name.Contains(configuration.AssemblyName));

            DotBoilApp.Services.TryAddSingleton<RazorLightEngine>(sp =>
            {
                return new RazorLightEngineBuilder()
                    .UseEmbeddedResourcesProject(assembly, configuration.RootNamespace)
                    .UseMemoryCachingProvider()
                    .Build();
            });

            DotBoilApp.Services.TryAddSingleton<IRazorRenderer, RazorRenderer>();

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
