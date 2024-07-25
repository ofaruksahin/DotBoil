using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.MassTransit.Configuration;
using DotBoil.MassTransit.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit
{
    internal class MassTransitModule : Module
    {
        public override async Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var configuration = builder.Configuration.GetConfigurations<MassTransitPersistenceConfiguration>();

            switch (configuration.PersistenceType)
            {
                case MassTransitPersistenceType.MySql:
                    await configuration.MySql.ConfigurePersistence(builder);
                    break;
                default:
                    throw new Exception("Not supported persistence type");
            }

            builder.Services.AddScoped<MassTransitDbContextSaveChangesInterceptor>();

            return builder;
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
