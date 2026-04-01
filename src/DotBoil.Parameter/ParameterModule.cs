using DotBoil.Dependency;
using DotBoil.Parameter.Endpoints;
using DotBoil.Parameter.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Parameter
{
    internal class ParameterModule : Module
    {
        public override string Name => "Parameter";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 0;

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

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            if (DotBoilApp.Host is WebApplication app)
            {
                app.MapParameterEndpoints();
            }
        }
    }
}
