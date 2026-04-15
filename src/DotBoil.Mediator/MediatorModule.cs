using DotBoil.Configuration;
using DotBoil.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Module = DotBoil.Dependency.Module;

namespace DotBoil.Mediator;

internal class MediatorModule : Module
{
    public override string Name => "Mediator";

    public override IEnumerable<string> DependsOn { get; } = new List<string>
    {
        "Parameter"
    };
    
    public override int Order { get; } = 10;

    public override Task AddModule()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.GetName().Name!.StartsWith("Microsoft.Build"))
                .Where(a => !a.GetName().Name!.StartsWith("Microsoft.CodeAnalysis"))
                .ToList();

            DotBoilApp.Services.AddMediatR(configure =>
            {
                configure.RegisterServicesFromAssemblies(assemblies.ToArray());
            });

            var mediatorOptions = DotBoilApp.Configuration.GetConfigurations<MediatorOptions>();

            foreach (var pipeline in mediatorOptions.Pipelines)
            {
                var pipelineType = AppDomain.CurrentDomain.FindType($"{pipeline}`2");

                if (pipelineType is null)
                    continue;

                DotBoilApp.Services.AddTransient(typeof(IPipelineBehavior<,>), pipelineType);
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"⚠️ Mediator registration error: {ex.Message}");
            
            DotBoilApp.Services.AddMediatR(configure =>
            {
                configure.RegisterServicesFromAssembly(Assembly.GetEntryAssembly()!);
            });
        }

        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        return Task.CompletedTask;
    }
}