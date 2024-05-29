using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIMemoryPersistenceOptions : UIPersistenceOptions
    {
        public string DatabaseName { get; set; }

        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            healthChecksUIBuilder.AddInMemoryStorage(options => { }, DatabaseName);
        }
    }
}
