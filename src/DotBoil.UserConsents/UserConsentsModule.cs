using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.UserConsents.Configurations;
using DotBoil.UserConsents.Endpoints;
using DotBoil.UserConsents.Persistence;
using DotBoil.UserConsents.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.UserConsents
{
    internal class UserConsentsModule : Module
    {
        public override string Name => "UserConsents";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 0;

        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<UserConsentsConfiguration>();

            DotBoilApp.Services.AddSingleton(configuration);

            DotBoilApp.Services.AddDbContext<UserConsentsDbContext>(options =>
            {
                options.UseMySQL(configuration.Persistence.ConnectionString);
            });

            DotBoilApp.Services.AddScoped<UserConsentsService>();
            DotBoilApp.Services.AddScoped<IUserConsentsService>(sp => sp.GetRequiredService<UserConsentsService>());

            return Task.CompletedTask;
        }

        public override async Task UseModule()
        {
            using var scope = DotBoilApp.Host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

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
                app.MapUserConsentsEndpoints();
            }
        }
    }
}
