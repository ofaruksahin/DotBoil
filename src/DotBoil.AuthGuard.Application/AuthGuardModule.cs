using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.AuthGuard.Application.Infrastructure.OpenIdDict.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;

namespace DotBoil.AuthGuard.Application;

public class AuthGuardModule : Module
{
    public override Task AddModule()
    {
        DotBoilApp.Services.AddScoped<ICurrentLanguage, CurrentLanguageService>();
        DotBoilApp.Services.AddScoped<IAuditUser, CurrentAuditUserService>();

        var openIdDictOptions = DotBoilApp.Configuration.GetConfigurations<OpenIdDictConfigurations>();
        
        DotBoilApp.Services.AddDbContext<OpenIdDictDbContext>(configure =>
        {
            configure.UseMySQL(openIdDictOptions.Core.ConnectionString);
            configure.UseOpenIddict();
        });

        DotBoilApp.Services.AddOpenIddict()
            .AddCore(configure =>
            {
                configure
                    .UseEntityFrameworkCore()
                    .UseDbContext<OpenIdDictDbContext>();
            })
            .AddClient(configure =>
            {
                if (openIdDictOptions.Client.EnableAuthorizationCodeFlow)
                    configure.AllowAuthorizationCodeFlow();
                if (openIdDictOptions.Client.EnableClientCredentialsFlow)
                    configure.AllowClientCredentialsFlow();
                if (openIdDictOptions.Client.EnableRefreshTokenFlow)
                    configure.AllowRefreshTokenFlow();
                if (openIdDictOptions.Client.EnableDeviceCodeFlow)
                    configure.AllowDeviceCodeFlow();
            })
            .AddServer(configure =>
            {
                if (openIdDictOptions.Server.AuthorizationEndpointUris.Any())
                    configure.SetAuthorizationEndpointUris(openIdDictOptions.Server.AuthorizationEndpointUris);
                if (openIdDictOptions.Server.LogoutEndpointUris.Any())
                    configure.SetLogoutEndpointUris(openIdDictOptions.Server.LogoutEndpointUris);
                if (openIdDictOptions.Server.TokenEndpointUris.Any())
                    configure.SetTokenEndpointUris(openIdDictOptions.Server.TokenEndpointUris);
                if (openIdDictOptions.Server.UserInfoEndpointUris.Any())
                    configure.SetUserinfoEndpointUris(openIdDictOptions.Server.UserInfoEndpointUris);
                if (openIdDictOptions.Server.RevocationEndpointUris.Any())
                    configure.SetRevocationEndpointUris(openIdDictOptions.Server.RevocationEndpointUris);
                if (openIdDictOptions.Server.DeviceEndpointUris.Any())
                    configure.SetDeviceEndpointUris(openIdDictOptions.Server.DeviceEndpointUris);

                if (openIdDictOptions.Server.EnableAuthorizationCodeFlow)
                    configure.AllowAuthorizationCodeFlow();
                if (openIdDictOptions.Server.EnableClientCredentialsFlow)
                    configure.AllowClientCredentialsFlow();
                if (openIdDictOptions.Server.EnableRefreshTokenFlow)
                    configure.AllowRefreshTokenFlow();

                configure
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                configure
                    .SetAccessTokenLifetime(openIdDictOptions.Server.AccessTokenLifetime)
                    .SetAuthorizationCodeLifetime(openIdDictOptions.Server.AuthorizationCodeLifetime)
                    .SetDeviceCodeLifetime(openIdDictOptions.Server.DeviceCodeLifetime)
                    .SetIdentityTokenLifetime(openIdDictOptions.Server.IdentityTokenLifetime)
                    .SetRefreshTokenLifetime(openIdDictOptions.Server.RefreshTokenLifetime)
                    .SetUserCodeLifetime(openIdDictOptions.Server.UserCodeLifetime);
            })
            .AddValidation(configure =>
            {
                configure.UseLocalServer();
                configure.UseAspNetCore();
            });

        DotBoilApp.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        
        return Task.CompletedTask;
    }

    public override Task UseModule()
    {
        ((WebApplication)DotBoilApp.Host).UseAuthentication();
        ((WebApplication)DotBoilApp.Host).UseAuthorization();
        return Task.CompletedTask;
    }
}