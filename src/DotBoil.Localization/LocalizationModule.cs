using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Localization.Configurations;
using DotBoil.Localization.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Localization
{
    internal class LocalizationModule : Module
    {
        public override Task AddModule()
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<LocalizationConfiguration>();

            DotBoilApp.Services.AddDbContext<LocalizationDbContext>(options =>
            {
                options.UseMySQL(configuration.Persistence.ConnectionString);
            });

            DotBoilApp.Services.AddSingleton<ILocalize, Localize>();

            return Task.CompletedTask;
        }

        public override async Task UseModule()
        {
            var scope = DotBoilApp.Host.Services.CreateScope();
            var localize = scope.ServiceProvider.GetRequiredService<ILocalize>();
            await localize.Initialize();
        }
    }
}
