using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health
{
    internal abstract class UIPersistenceOptions
    {
       public abstract void AddPersistence(HealthChecksUIBuilder healthChecksUIBuilder);
    }
}
