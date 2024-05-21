using DotBoil.Configuration;
using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace DotBoil
{
    public static class Bootstrapper
    {
        public static async Task<WebApplicationBuilder> AddDotBoil(this WebApplicationBuilder builder, params Assembly[] assemblies)
        {
            await builder.AddDotBoilConfigurations(assemblies);
            await builder.AddDotBoilDependencies(assemblies);

            return builder;
        }

        public static async Task<WebApplication> UseDotBoil(this WebApplication app, params Assembly[] assemblies)
        {
            await app.UseDotBoilDependencies(assemblies);
            return app;
        }
    }
}
