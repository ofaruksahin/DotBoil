using DotBoil.Configuration;
using DotBoil.EFCore.Attributes;
using DotBoil.EFCore.Configurations;
using DotBoil.EFCore.Exceptions;
using DotBoil.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.EFCore
{
    public abstract class EFCoreDbContext : DbContext
    {
        protected IServiceProvider _serviceProvider;

        protected EFCoreDbContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected abstract void ConfigureDatabaseProvider(DbContextOptionsBuilder optionsBuilder);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var contexts = DotBoilApp.Configuration.GetConfigurations<EFCoreConfiguration>();

            var interceptors = contexts
                .Contexts
                .FirstOrDefault(context => context.TypeName == GetType().FullName)?
                .Interceptors
                .Select(interceptor => AppDomain.CurrentDomain.FindType(interceptor))
                .ToList() ?? new List<Type>();

            foreach (var interceptorType in interceptors)
            {
                var interceptor = (IInterceptor)_serviceProvider.GetRequiredService(interceptorType);
                optionsBuilder.AddInterceptors(interceptor);
            }

            ConfigureDatabaseProvider(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var applyConfigurationMethod = modelBuilder.GetType().GetMethod(nameof(modelBuilder.ApplyConfiguration));
            if (applyConfigurationMethod == null)
                throw new EFCoreApplyConfigurationMethodNotFoundException();

            var entityTypeConfigurations = AppDomain.CurrentDomain.FindTypesWithInterface(typeof(IEntityTypeConfiguration<>));

            foreach (var type in entityTypeConfigurations)
            {
                var entityTypeConfigurationAttribute = type.GetCustomAttribute(typeof(DotBoilEntityTypeConfigurationAttribute)) as DotBoilEntityTypeConfigurationAttribute;

                if (entityTypeConfigurationAttribute is null)
                    continue;

                if (entityTypeConfigurationAttribute.DbContextType == GetType())
                {
                    var entityTypeConfiguration = _serviceProvider.GetService(type);
                    applyConfigurationMethod.MakeGenericMethod(type.BaseType.GetGenericArguments().FirstOrDefault())?.Invoke(modelBuilder, new[] {entityTypeConfiguration});
                }
            }
        }
    }
}
