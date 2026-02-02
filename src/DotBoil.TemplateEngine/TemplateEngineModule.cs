using DotBoil.Configuration;
using DotBoil.TemplateEngine.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RazorLight;
using Module = DotBoil.Dependency.Module;

namespace DotBoil.TemplateEngine
{
    internal class TemplateEngineModule : Module
    {
        public override string Name => "TemplateEngine";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 0;

        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<RazorViewEngineConfiguration>();

            var assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
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
