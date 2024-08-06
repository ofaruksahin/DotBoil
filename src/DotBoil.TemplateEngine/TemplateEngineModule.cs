using DotBoil.Configuration;
using DotBoil.TemplateEngine.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using Module = DotBoil.Dependency.Module;

namespace DotBoil.TemplateEngine
{
    internal class TemplateEngineModule : Module
    {
        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<RazorViewEngineConfiguration>();

            var assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(ass => ass.GetName().Name.Contains(configuration.AssemblyName));

            DotBoilApp.Services.AddSingleton<RazorLightEngine>(sp =>
            {
                return new RazorLightEngineBuilder()
                    .UseEmbeddedResourcesProject(assembly, configuration.RootNamespace)
                    .UseMemoryCachingProvider()
                    .Build();
            });

            DotBoilApp.Services.AddSingleton<IRazorRenderer, RazorRenderer>();

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
