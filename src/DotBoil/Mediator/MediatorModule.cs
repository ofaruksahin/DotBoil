using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Mediator
{
    internal class MediatorModule : Module
    {
        public override Task AddModule()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            DotBoilApp.Services.AddMediatR(configure =>
            {
                configure.RegisterServicesFromAssemblies(assemblies);
            });

            var mediatorOptions = DotBoilApp.Configuration.GetConfigurations<MediatorOptions>();

            foreach (var pipeline in mediatorOptions.Pipelines)
            {
                var pipelineType = AppDomain.CurrentDomain.FindType($"{pipeline}`2");

                if (pipelineType is null)
                    continue;

                DotBoilApp.Services.AddTransient(typeof(IPipelineBehavior<,>), pipelineType);
            }

            return Task.CompletedTask;
        }

        public override Task UseModule()
        {
            return Task.CompletedTask;
        }
    }
}
