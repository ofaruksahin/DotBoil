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
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddSwaggerGen(configure =>
            {
                var options = builder.Configuration.GetConfigurations<SwagOptions>();
                var xmlPath = Path.Combine(AppContext.BaseDirectory, options.XmlFile);
                configure.IncludeXmlComments(xmlPath);
            });
            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<SwagOptions>();

            app.UseSwagger();
            app.UseSwaggerUI(configure =>
            {
                foreach (var version in options.Versions)
                    configure.SwaggerEndpoint(string.Format("/swagger/{0}/swagger.json", version.VersionName), version.VersionDescription);
            });
            return Task.FromResult(app);
        }
    }
}
