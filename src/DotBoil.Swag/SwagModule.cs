using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;

namespace DotBoil.Swag
{
    internal class SwagModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
