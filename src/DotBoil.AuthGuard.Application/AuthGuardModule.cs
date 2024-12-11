using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application;

public class AuthGuardModule : Module
{
    public override Task AddModule()
    {
        DotBoilApp.Services.AddScoped<ICurrentLanguage, CurrentLanguageService>();
        DotBoilApp.Services.AddScoped<IAuditUser, CurrentAuditUserService>();
        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        return Task.CompletedTask;
    }
}