using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Health.Configuration;
using DotBoil.Health.Configuration.UI;
using DotBoil.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health
{
    internal class HealthModule : Module
    {
        private static HealthOptions GetHealthOptions() => 
            DotBoilApp.Configuration.GetConfigurations<HealthOptions>();

        private static readonly HealthCheckOptions DefaultHealthCheckOptions = new()
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        };

        public override Task AddModule()
        {
            var healthOptions = GetHealthOptions();

            if (HasHealthCheckEndpoint(healthOptions))
            {
                AddHealthChecks();
            }

            if (healthOptions.UI is not null)
            {
                AddHealthChecksUI(healthOptions.UI);
            }

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            if (DotBoilApp.Host is not WebApplication app)
                return Task.CompletedTask;

            var healthOptions = GetHealthOptions();

            if (HasHealthCheckEndpoint(healthOptions))
            {
                app.UseHealthChecks(healthOptions.Url, DefaultHealthCheckOptions);
            }

            if (healthOptions.UI is not null)
            {
                app.UseHealthChecksUI(setup => setup.UIPath = healthOptions.UI.Url);
            }

            return Task.CompletedTask;
        }

        private static bool HasHealthCheckEndpoint(HealthOptions options) => 
            !string.IsNullOrEmpty(options.Url);

        private static void AddHealthChecks()
        {
            var healthCheckBuilder = DotBoilApp.Services.AddHealthChecks();
            var configureType = AppDomain.CurrentDomain.FindTypeWithBaseType(typeof(ConfigureHealthCheck));

            if (configureType is null)
                return;

            var configureInstance = Activator.CreateInstance(configureType) as ConfigureHealthCheck;
            configureInstance?.Configure(healthCheckBuilder);
        }

        private static void AddHealthChecksUI(HealthUIOptions uiOptions)
        {
            var healthCheckUIBuilder = DotBoilApp.Services.AddHealthChecksUI(settings =>
            {
                foreach (var service in uiOptions.Services)
                {
                    settings.AddHealthCheckEndpoint(service.Name, service.Uri);
                }
            });

            var persistenceOptions = GetPersistenceOptions(uiOptions);
            persistenceOptions?.AddPersistence(healthCheckUIBuilder);
        }

        private static UIPersistenceOptions GetPersistenceOptions(HealthUIOptions uiOptions) =>
            uiOptions.InMemory as UIPersistenceOptions ?? 
            uiOptions.MySql as UIPersistenceOptions;
    }
}