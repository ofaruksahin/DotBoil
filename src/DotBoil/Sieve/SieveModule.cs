using DotBoil.Dependency;
using DotBoil.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Sieve.Services;

namespace DotBoil.Sieve
{
    internal class SieveModule : Module
    {
        public override Task AddModule()
        {
            var processors = AppDomain.CurrentDomain.FindTypesWithBaseType(typeof(SieveProcessor)) ?? new List<Type>();
            var filters = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(ISieveCustomFilterMethods)) ?? new List<Type>();
            var sorts = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(ISieveCustomSortMethods)) ?? new List<Type>();

            foreach (var proccessor in processors)
                DotBoilApp.Services.AddScoped(typeof(ISieveProcessor), proccessor);

            foreach (var filter in filters)
                DotBoilApp.Services.AddScoped(typeof(ISieveCustomFilterMethods), filter);

            foreach (var sort in sorts)
                DotBoilApp.Services.AddScoped(typeof(ISieveCustomSortMethods), sort);

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
