using System.Text;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
        DotBoilApp.Services.AddScoped<IMenuService, MenuService>();
        DotBoilApp.Services.AddScoped<IPermissionService, PermissionService>();

        var jwtOptions = DotBoilApp.Configuration.GetConfigurations<JwtOptions>();
        
        DotBoilApp
            .Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateActor = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });
        
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