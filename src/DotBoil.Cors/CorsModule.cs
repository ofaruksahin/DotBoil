using DotBoil.Configuration;
using DotBoil.Cors.Configuration;
using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Cors
{
    internal class CorsModule : Module
    {
        public override string Name => "Cors";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 0;

        public override Task AddModule()
        {
            var options = DotBoilApp.Configuration.GetConfigurations<CorsOptions>();

            DotBoilApp.Services.AddCors(configure =>
            {
                configure.AddPolicy(options.PolicyName, policy =>
                {
                    policy.WithOrigins(options.Origins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return Task.CompletedTask;
        }

        public override Task<WebApplication> UseModule()
        {
            var app = DotBoilApp.Host as WebApplication;
            using var scope = app.Services.CreateScope();
            var options = scope.ServiceProvider.GetService<CorsOptions>();

            app.UseCors(options.PolicyName);

            return Task.FromResult(app);
        }
    }
}
