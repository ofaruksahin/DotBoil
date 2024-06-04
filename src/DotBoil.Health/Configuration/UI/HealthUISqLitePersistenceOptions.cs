using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUISqLitePersistenceOptions : UIPersistenceOptions
    {
        public string ConnectionString { get; set; }

        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            healthChecksUIBuilder.AddSqliteStorage(ConnectionString);
        }
    }
}
