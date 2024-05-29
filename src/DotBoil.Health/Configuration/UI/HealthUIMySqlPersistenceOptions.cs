using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIMySqlPersistenceOptions : UIPersistenceOptions
    {
        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
