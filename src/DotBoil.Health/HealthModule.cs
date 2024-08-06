using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Health.Configuration;
using DotBoil.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health
{
    internal class HealthModule : Module
    {
        public override Task AddModule()
        {
            var healthOptions = DotBoilApp.Configuration.GetConfigurations<HealthOptions>();

            if (!string.IsNullOrEmpty(healthOptions.Url))
            {
                var healthCheckBuilder = DotBoilApp.Services.AddHealthChecks();

                var configureType = AppDomain.CurrentDomain.FindTypeWithBaseType(typeof(ConfigureHealthCheck));

                if (configureType is not null)
                {
                    var configureInstance = (ConfigureHealthCheck)Activator.CreateInstance(configureType);

                    configureInstance.Configure(healthCheckBuilder);
                }
            }

            if (healthOptions.UI is not null)
            {
                var healthCheckUIBuilder = DotBoilApp.Services.AddHealthChecksUI(settings =>
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
            }

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            var app = DotBoilApp.Host as WebApplication;
            using var scope = app.Services.CreateScope();
            var healthOptions = scope.ServiceProvider.GetService<HealthOptions>();

            if (!string.IsNullOrEmpty(healthOptions.Url))
            {
                app.UseHealthChecks(healthOptions.Url, new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }

            if (healthOptions.UI is not null)
            {
                app.UseHealthChecksUI(setup =>
                {
                    setup.UIPath = healthOptions.UI.Url;
                });
            }

            return Task.FromResult(app);
        }
    }
}