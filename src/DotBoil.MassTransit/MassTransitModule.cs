using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.MassTransit.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit
{
    internal class MassTransitModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var massTransitOptions = builder.Configuration.GetConfigurations<MassTransitOptions>();

            builder.Services.AddDbContext<MassTransitDbContext>(configure =>
            {
                switch (massTransitOptions.PersistenceType)
                {
                    case PersistenceType.MySql:
                        massTransitOptions.MySql.ConfigurePersistence(configure);
                        break;
                }
            });

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
