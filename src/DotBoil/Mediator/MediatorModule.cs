using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Mediator
{
    internal class MediatorModule : Module
    {
        public override Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder.Services.AddMediatR(configure =>
            {
                configure.RegisterServicesFromAssemblies(assemblies);
            });

            var mediatorOptions = builder.Configuration.GetConfigurations<MediatorOptions>();

            foreach (var pipeline in mediatorOptions.Pipelines)
            {
                var pipelineType = AppDomain.CurrentDomain.FindType($"{pipeline}`2");

                if (pipelineType is null)
                    continue;

                builder.Services.AddTransient(typeof(IPipelineBehavior<,>), pipelineType);
            }

            return Task.FromResult(builder);
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }
    }
}
