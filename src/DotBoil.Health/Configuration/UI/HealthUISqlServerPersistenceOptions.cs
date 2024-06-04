using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUISqlServerPersistenceOptions : UIPersistenceOptions
    {
        public string ConnectionString { get; set; }

        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            healthChecksUIBuilder.AddSqlServerStorage(ConnectionString);
        }
    }
}
