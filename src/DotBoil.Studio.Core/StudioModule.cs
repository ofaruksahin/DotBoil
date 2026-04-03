using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Studio.Core.Persistence;
using DotBoil.Studio.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;

namespace DotBoil.Studio.Core;

internal class StudioModule : Module
{
    public override string Name => "Studio";
    public override IEnumerable<string> DependsOn => new[] { "EFCore" };
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
            .AddScoped<IUIConfigService, UIConfigService>();

        DotBoilApp
            .Services
            .AddScoped<IAuditUser, AuditUser>();

        DotBoilApp
            .Services
            .AddHttpClient("DotBoilStudioClient");

        return Task.CompletedTask;
    }

    public override async Task UseModule()
    {
        (DotBoilApp
                .Host as WebApplication)
            .MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        using var scope = DotBoilApp.Host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StudioDbContext>();

        try
        {
            await dbContext.Database.EnsureCreatedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Studio] Veritabanı oluşturma hatası: {ex.Message}");
        }
    }
}