using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUISqLitePersistenceOptions : UIPersistenceOptions
    {
        public override void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
