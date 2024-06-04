using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace DotBoil.Sieve
{
    internal class SieveModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var processors = AppDomain.CurrentDomain.FindTypesWithBaseType(typeof(SieveProcessor));
            var filters = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(ISieveCustomFilterMethods));
            var sorts = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(ISieveCustomSortMethods));

            foreach (var proccessor in processors)
                builder.Services.AddScoped(typeof(ISieveProcessor), proccessor);

            foreach (var filter in filters)
                builder.Services.AddScoped(typeof(ISieveCustomFilterMethods), filter);

            foreach (var sort in sorts)
                builder.Services.AddScoped(typeof(ISieveCustomSortMethods), sort);

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
