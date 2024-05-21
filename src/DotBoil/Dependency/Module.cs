using Microsoft.AspNetCore.Builder;

namespace DotBoil.Dependency
{
    public abstract class Module
    {
        public abstract Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder);
        public abstract Task<WebApplication> UseModule(WebApplication app);
    }
}
