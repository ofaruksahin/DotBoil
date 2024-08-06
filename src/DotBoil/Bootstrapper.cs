using DotBoil.Configuration;
using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace DotBoil
{
    public static class Bootstrapper
    {
        public static async Task<WebApplicationBuilder> AddDotBoil(this WebApplicationBuilder builder, params Assembly[] assemblies)
        {
            DotBoilApp.Configuration = builder.Configuration;
            DotBoilApp.Logging = builder.Logging;
            DotBoilApp.Services = builder.Services;

            await ConfigurationBootstrapper.AddDotBoilConfigurations(assemblies);
            await DependencyBootstrapper.AddDotBoilDependencies(assemblies);

            return builder;
        }

        public static async Task<HostApplicationBuilder> AddDotBoil(this HostApplicationBuilder builder, params Assembly[] assemblies)
        {
            DotBoilApp.Configuration = builder.Configuration;
            DotBoilApp.Logging = builder.Logging;
            DotBoilApp.Services = builder.Services;

            await ConfigurationBootstrapper.AddDotBoilConfigurations(assemblies);
            await DependencyBootstrapper.AddDotBoilDependencies(assemblies);

            return builder;
        }

        public static async Task<IHost> UseDotBoil(this IHost app, params Assembly[] assemblies)
        {
            DotBoilApp.Host = app;

            await DependencyBootstrapper.UseDotBoilDependencies(assemblies);

            return app;
        }
    }

    public static class DotBoilApp
    {
        public static IConfiguration Configuration { get; internal set; }
        public static ILoggingBuilder Logging { get; internal set; }
        public static IServiceCollection Services { get; internal set; }
        public static IHost Host { get; internal set; }
    }
}
