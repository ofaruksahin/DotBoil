using DotBoil.Swag.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DotBoil.Swag
{
    internal class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private IServiceProvider _serviceProvider;

        public ConfigureSwaggerOptions(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            using var scope = _serviceProvider.CreateScope();
            var swagOptions = scope.ServiceProvider.GetRequiredService<SwagOptions>();

            foreach (var version in swagOptions.Versions)
                options.SwaggerDoc(version.VersionName, CreateInfoForApiVersion(swagOptions.Contact, version));
        }

        private OpenApiInfo CreateInfoForApiVersion(SwagContactOptions contactOptions, SwagVersionOptions versionOptions)
        {
            return new OpenApiInfo
            {
                Version = versionOptions.VersionName,
                Title = string.Concat(contactOptions.Title, Environment.NewLine, versionOptions.Deprecated ? "This API version has been deprecated" : string.Empty),
                Description = contactOptions.Description,
                Contact = new OpenApiContact
                {
                    Name = contactOptions.ContactName,
                    Email = contactOptions.ContactEmail,
                    Url = !string.IsNullOrEmpty(contactOptions.ContactUrl) ? new Uri(contactOptions.ContactUrl) : null
                },
                License = !string.IsNullOrEmpty(contactOptions.LicenseName) ? new OpenApiLicense { Name = contactOptions.LicenseName } : null
            };
        }
    }
}
