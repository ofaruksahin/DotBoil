using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Swag.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotBoil.Swag
{
    internal class SwagModule : Module
    {
        public override Task AddModule()
        {
            DotBoilApp.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            DotBoilApp.Services.AddSwaggerGen(configure =>
            {
                var options = DotBoilApp.Configuration.GetConfigurations<SwagOptions>();
                var xmlPath = Path.Combine(AppContext.BaseDirectory, options.XmlFile);
                configure.IncludeXmlComments(xmlPath);
            });

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            var app = DotBoilApp.Host as WebApplication;
            using var scope = app.Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<SwagOptions>();

            app.UseSwagger();
            app.UseSwaggerUI(configure =>
            {
                foreach (var version in options.Versions)
                    configure.SwaggerEndpoint(string.Format("/swagger/{0}/swagger.json", version.VersionName), version.VersionDescription);
            });

            return Task.CompletedTask;
        }
    }
}
