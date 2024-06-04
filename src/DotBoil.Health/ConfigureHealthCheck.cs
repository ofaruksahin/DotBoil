using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Health
{
    public abstract class ConfigureHealthCheck
    {
        public abstract void Configure(WebApplicationBuilder webApplicationBuilder, IHealthChecksBuilder healthCheckBuilder);
    }
}
