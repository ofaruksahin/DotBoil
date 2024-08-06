using DotBoil.Configuration;
using DotBoil.TemplateEngine.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;
using Module = DotBoil.Dependency.Module;

namespace DotBoil.TemplateEngine
{
    internal class TemplateEngineModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration.GetConfigurations<RazorViewEngineConfiguration>();

            var assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(ass => ass.GetName().Name.Contains(configuration.AssemblyName));

            builder.Services.AddSingleton<RazorLightEngine>(sp =>
            {
                return new RazorLightEngineBuilder()
                    .UseEmbeddedResourcesProject(assembly, configuration.RootNamespace)
                    .UseMemoryCachingProvider()
                    .Build();
            });

            builder.Services.AddSingleton<IRazorRenderer, RazorRenderer>();

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
