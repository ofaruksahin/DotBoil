using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application;

public class AuthGuardModule : Module
{
    public override Task AddModule()
    {
        DotBoilApp.Services.AddScoped<ICurrentLanguage, CurrentLanguageService>();
        DotBoilApp.Services.AddScoped<IAuditUser, CurrentAuditUserService>();
        DotBoilApp.Services.AddScoped<IUserService, UserService>();
        DotBoilApp.Services.AddScoped<IExternalSignInManager, ExternalSignInManager>()
            .AddHttpClient();
        DotBoilApp.Services.AddScoped<IJwtService, JwtService>();

        DotBoilApp
            .Services
            .AddAuthorization();
        
        return Task.CompletedTask;
    }

    public override async Task UseModule()
    {
        ((WebApplication)DotBoilApp.Host).UseAuthentication();
        ((WebApplication)DotBoilApp.Host).UseAuthorization();
    }
}