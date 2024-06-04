using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIMySqlPersistenceOptions : UIPersistenceOptions
    {
        public string ConnectionString { get; set; }

        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            healthChecksUIBuilder.AddMySqlStorage(ConnectionString);
        }
    }
}
