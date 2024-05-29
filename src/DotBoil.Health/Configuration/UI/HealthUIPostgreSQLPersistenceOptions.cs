using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIPostgreSQLPersistenceOptions : UIPersistenceOptions
    {
        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
