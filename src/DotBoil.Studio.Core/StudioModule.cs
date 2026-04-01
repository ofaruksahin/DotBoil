using DotBoil.Dependency;
using DotBoil.Studio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;

namespace DotBoil.Studio.Core;

internal class StudioModule : Module
{
    public override string Name => "Studio";
    public override IEnumerable<string> DependsOn => Enumerable.Empty<string>();
    public override int Order { get; } = 2;
    public override Task AddModule()
    {
        DotBoilApp
            .Services
            .AddRazorComponents()
            .AddInteractiveServerComponents();
        
        DotBoilApp
            .Services
            .AddMudServices();
        
        DotBoilApp
            .Services
            .AddMudExtensions();

        DotBoilApp
            .Services
            .AddScoped<JwtAuthService>()
            .AddHttpClient();

        DotBoilApp
            .Services
            .AddHttpClient("DotBoilStudioClient");

        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        (DotBoilApp
                .Host as WebApplication)
            .MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();
        
        return Task.CompletedTask;
    }
}