using DotBoil.Dependency;
using DotBoil.Parameter.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Parameter
{
    internal class ParameterModule : Module
    {
        public override Task AddModule()
        {
            DotBoilApp.Services.AddDbContext<ParameterDbContext>();
            DotBoilApp.Services.AddSingleton<IParameterManager, ParameterManager>();

            return Task.CompletedTask;
        }

        public override async Task UseModule()
        {
            using var scope = DotBoilApp.Host.Services.CreateScope();
            scope.ServiceProvider.GetService<IParameterManager>();
            
            var context = scope.ServiceProvider.GetRequiredService<ParameterDbContext>();
            await context.Database.MigrateAsync();
        }
    }
}
