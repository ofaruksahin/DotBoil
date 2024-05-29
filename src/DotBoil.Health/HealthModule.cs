using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Health.Configuration;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DotBoil.Health
{
    internal class HealthModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var healthOptions = builder.Configuration.GetConfigurations<HealthOptions>();

            var healthCheckBuilder = builder.Services.AddHealthChecks();

            var healthCheckUIBuilder = builder.Services.AddHealthChecksUI(settings =>
            {
                healthOptions.UI.Services.ForEach(service => settings.AddHealthCheckEndpoint(service.Name, service.Uri));
            });

            switch (healthOptions.UI.PersistenceType)
            {
                case Configuration.UI.PersistenceType.InMemory:
                    healthOptions.UI.InMemory.AddPersistence(healthCheckUIBuilder);
                    break;
                case Configuration.UI.PersistenceType.SqlServer:
                    healthOptions.UI.SqlServer.AddPersistence(healthCheckUIBuilder);
                    break;
                case Configuration.UI.PersistenceType.SqLite:
                    healthOptions.UI.SqLite.AddPersistence(healthCheckUIBuilder);
                    break;
                case Configuration.UI.PersistenceType.PostgreSQL:
                    healthOptions.UI.PostgreSQL.AddPersistence(healthCheckUIBuilder);
                    break;
                case Configuration.UI.PersistenceType.MySql:
                    healthOptions.UI.MySql.AddPersistence(healthCheckUIBuilder);
                    break;
            }

            return Task.FromResult(builder);
        }
        
        public override Task<WebApplication> UseModule(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var healthOptions = scope.ServiceProvider.GetService<HealthOptions>();

            app.UseHealthChecks(healthOptions.Url, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHealthChecksUI(setup =>
            {
                setup.UIPath = healthOptions.UI.Url;
            });

            return Task.FromResult(app);
        }
    }
}