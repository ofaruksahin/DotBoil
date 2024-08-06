using Asp.Versioning;
using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Versioning.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Versioning
{
    internal class VersioningModule : Module
    {
        public override Task AddModule()
        {
            var versioningOptions = DotBoilApp.Configuration.GetConfigurations<VersioningOptions>();

            var apiVersionReaders = new List<IApiVersionReader>();

            if (versioningOptions.UrlSegmentApiVersioningEnable)
                apiVersionReaders.Add(new UrlSegmentApiVersionReader());

            if (versioningOptions.HeaderApiVersioningEnable)
                apiVersionReaders.Add(new HeaderApiVersionReader(versioningOptions.HeaderApiVersioningHeaderName));

            DotBoilApp.Services.AddApiVersioning(configure =>
            {
                configure.DefaultApiVersion = new ApiVersion(
                    versioningOptions.DefaultMajorVersion,
                    versioningOptions.DefaultMinorVersion);
                configure.ReportApiVersions = true;
                configure.AssumeDefaultVersionWhenUnspecified = true;
                configure.ApiVersionReader = ApiVersionReader.Combine(apiVersionReaders);
            }).AddApiExplorer(configure =>
            {
                configure.GroupNameFormat = "'v'V";
                configure.SubstituteApiVersionInUrl = true;
            });

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
