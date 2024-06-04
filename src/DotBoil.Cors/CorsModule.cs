using DotBoil.Configuration;
using DotBoil.Cors.Configuration;
using DotBoil.Dependency;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Cors
{
    internal class CorsModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var options = builder.Configuration.GetConfigurations<CorsOptions>();

            builder.Services.AddCors(configure =>
            {
                configure.AddPolicy(options.PolicyName, policy =>
                {
                    policy.WithOrigins(options.Origins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var options = scope.ServiceProvider.GetService<CorsOptions>();

            app.UseCors(options.PolicyName);

            return Task.FromResult(app);
        }
    }
}
