using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.EFCore.Configurations;
using DotBoil.EFCore.Exceptions;
using DotBoil.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotBoil.EFCore
{
    internal class EFCoreModule : Module
    {
        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<EFCoreConfiguration>();

            var interceptors = configuration
                .Contexts
                .Select(context => context.Interceptors)
                .SelectMany(interceptor => interceptor)
                .Select(interceptor => AppDomain.CurrentDomain.FindType(interceptor))
                .ToList();

            foreach (var interceptor in interceptors)
                DotBoilApp.Services.TryAddScoped(interceptor);

            var loaders = AppDomain.CurrentDomain.FindTypesWithBaseType(typeof(EFCoreDbContextLoader));

            if (loaders == null || !loaders.Any())
                throw new EFCoreDbContextLoaderException();

            foreach (var loader in loaders)
            {
                var loaderInstance = (EFCoreDbContextLoader)Activator.CreateInstance(loader);

                if (loaderInstance == null)
                    throw new EFCoreDbContextLoaderException();

                loaderInstance.LoadDbContext(DotBoilApp.Configuration, DotBoilApp.Services);
            }

            var entityTypeConfigurations = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(IEntityTypeConfiguration<>));

            foreach (var entityTypeConfiguration in entityTypeConfigurations)
                DotBoilApp.Services.TryAddSingleton(entityTypeConfiguration);

            DotBoilApp.Services.AddScoped(typeof(IRepository<,>), typeof(EFCoreRepository<,>));

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
