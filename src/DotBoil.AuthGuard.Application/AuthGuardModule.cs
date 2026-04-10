using System.Text;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Endpoints;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.AuthGuard.Application.Infrastructure.Services;
using DotBoil.Configuration;
using DotBoil.Dependency;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DotBoil.AuthGuard.Application;

public class AuthGuardModule : Module
{
    public override string Name => "AuthGuard";
    public override IEnumerable<string> DependsOn => new List<string>
    {
        "Mediator",
        "Caching",
        "Cors",
        "EFCore",
        "Localization",
        "Logging",
        "Mapper",
        "MassTransit",
        "Parameter",
        "Validator"
    };

    public override int Order { get; } = 2;

    public override Task AddModule()
    {
        DotBoilApp.Services.AddScoped<ICurrentLanguage, CurrentLanguageService>();
        DotBoilApp.Services.AddScoped<IAuditUser, CurrentAuditUserService>();
        DotBoilApp.Services.AddKeyedScoped<IUserService, EmailPasswordBasedLogin>("EmailPasswordBasedLogin");
        DotBoilApp.Services.AddScoped<IExternalSignInManager, ExternalSignInManager>()
            .AddHttpClient();
        DotBoilApp.Services.AddScoped<IJwtService, JwtService>();
        DotBoilApp.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        DotBoilApp.Services.AddScoped<IMenuService, MenuService>();
        DotBoilApp.Services.AddScoped<IPermissionService, PermissionService>();
        DotBoilApp.Services.AddScoped<ITokenPermissionService>(serviceProvider =>
            (ITokenPermissionService)serviceProvider.GetRequiredService<IPermissionService>());

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
        var app = (WebApplication)DotBoilApp.Host;
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapAuthGuardEndpoints();
        app.MapApiEndpointEndpoints();
        app.MapAppModuleEndpoints();
        app.MapExternalLoginEndpoints();
        app.MapMenuEndpoints();
        app.MapRoleEndpoints();
        app.MapUserEndpoints();

        using var scope = DotBoilApp.Host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<DotBoilAuthGuardDbContext>();

        try
        {
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        var roleRepo = scope.ServiceProvider.GetService<IRepository<Role, DotBoilAuthGuardDbContext>>();
        var appModuleRepo = scope.ServiceProvider.GetService<IRepository<AppModule, DotBoilAuthGuardDbContext>>();
        var menuRepo = scope.ServiceProvider.GetService<IRepository<Menu, DotBoilAuthGuardDbContext>>();
        var roleMenuRepo = scope.ServiceProvider.GetService<IRepository<RoleMenu, DotBoilAuthGuardDbContext>>();
        var roleAppModuleRepo = scope.ServiceProvider.GetService<IRepository<RoleAppModule, DotBoilAuthGuardDbContext>>();

        var role = await roleRepo
            .Get()
            .FirstOrDefaultAsync(r => r.Name == "Admin");

        if (role is null)
        {
            role = new Role
            {
                Name = "Admin",
                IsDefault = false,
                CreateUser = "SYSTEM",
                CreateTime = DateTime.Now
            };

            await roleRepo.AddAsync(role);
            await roleRepo.SaveChangesAsync();
        }

        if (role.Id > 0)
        {
            var appModule = await appModuleRepo
                .Get()
                .FirstOrDefaultAsync(am => am.Name == "AuthGuard");

            if (appModule is null)
            {
                appModule = new AppModule
                {
                    Name = "AuthGuard",
                    Description = "This module is responsible for user session and authorization processes.",
                    CreateUser = "SYSTEM",
                    CreateTime = DateTime.Now
                };
                
                await appModuleRepo.AddAsync(appModule);
                await appModuleRepo.SaveChangesAsync();
            }

            if (appModule.Id > 0)
            {
                var roleAppModules = await roleAppModuleRepo
                    .Get()
                    .Where(am => am.AppModuleId == appModule.Id)
                    .ToListAsync();

                if (appModule != null)
                {
                    if (!roleAppModules.Any(r => r.RoleId == role.Id))
                    {
                        var roleAppModule = new RoleAppModule
                        {
                            AppModuleId = appModule.Id,
                            RoleId = role.Id,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        };
                        await roleAppModuleRepo.AddAsync(roleAppModule);
                        await roleAppModuleRepo.SaveChangesAsync();
                    }

                    var menuList = new List<Menu>
                    {
                        new Menu
                        {
                            Header = "AuthGuard",
                            Name = "Kullanıcılar",
                            Icon = "<g><rect fill=\"none\" height=\"24\" width=\"24\"/></g><g><g><path d=\"M6.32,13.01c0.96,0.02,1.85,0.5,2.45,1.34C9.5,15.38,10.71,16,12,16c1.29,0,2.5-0.62,3.23-1.66 c0.6-0.84,1.49-1.32,2.45-1.34C16.96,11.78,14.08,11,12,11C9.93,11,7.04,11.78,6.32,13.01z\"/><path d=\"M4,13L4,13c1.66,0,3-1.34,3-3c0-1.66-1.34-3-3-3s-3,1.34-3,3C1,11.66,2.34,13,4,13z\"/><path d=\"M20,13L20,13c1.66,0,3-1.34,3-3c0-1.66-1.34-3-3-3s-3,1.34-3,3C17,11.66,18.34,13,20,13z\"/><path d=\"M12,10c1.66,0,3-1.34,3-3c0-1.66-1.34-3-3-3S9,5.34,9,7C9,8.66,10.34,10,12,10z\"/><path d=\"M21,14h-3.27c-0.77,0-1.35,0.45-1.68,0.92C16.01,14.98,14.69,17,12,17c-1.43,0-3.03-0.64-4.05-2.08 C7.56,14.37,6.95,14,6.27,14H3c-1.1,0-2,0.9-2,2v4h7v-2.26c1.15,0.8,2.54,1.26,4,1.26s2.85-0.46,4-1.26V20h7v-4 C23,14.9,22.1,14,21,14z\"/></g></g>",
                            Path = "/users",
                            Rank = 4,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now,
                        },
                        new Menu()
                        {
                            Header = "AuthGuard",
                            Name = "Roller",
                            Icon = "<g><rect fill=\"none\" height=\"24\" width=\"24\"/></g><g><g><path d=\"M21,9v2h-2V3h-2v2h-2V3h-2v2h-2V3H9v2H7V3H5v8H3V9H1v12h9v-3c0-1.1,0.9-2,2-2s2,0.9,2,2v3h9V9H21z M11,12H9V9h2V12z M15,12h-2V9h2V12z\"/></g></g>",
                            Path = "/roles",
                            Rank = 1,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now,
                        },
                        new Menu()
                        {
                            Header = "AuthGuard",
                            Name = "Modüller",
                            Icon = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M18 8c-2.21 0-4 1.79-4 4s1.79 4 4 4 4-1.79 4-4-1.79-4-4-4zm-2.5 4c0-1.38 1.12-2.5 2.5-2.5.42 0 .8.11 1.15.29l-3.36 3.36c-.18-.35-.29-.73-.29-1.15zm2.5 2.5c-.42 0-.8-.11-1.15-.29l3.36-3.36c.18.35.29.73.29 1.15 0 1.38-1.12 2.5-2.5 2.5zM17 18H7V6h10v1h2V3c0-1.1-.9-2-2-2H7c-1.1 0-2 .9-2 2v18c0 1.1.9 2 2 2h10c1.1 0 2-.9 2-2v-4h-2v1z\"/>",
                            Path = "/modules",
                            Rank = 0,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "AuthGuard",
                            Name = "Menüler",
                            Icon = "<path d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M3 18h18v-2H3v2zm0-5h18v-2H3v2zm0-7v2h18V6H3z\"/>",
                            Path = "/menu",
                            Rank = 2,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "AuthGuard",
                            Name = "Servisler",
                            Icon = "<g><path d=\"M0,0h24v24H0V0z\" fill=\"none\"/><path d=\"M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z\"/></g>",
                            Path = "/api-endpoints",
                            Rank = 3,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "Sistem",
                            Name = "Parametreler",
                            Icon = "<path clip-rule=\"evenodd\" d=\"M0 0h24v24H0z\" fill=\"none\"/><path d=\"M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z\"/>",
                            Path = "/parameters",
                            Rank = 99,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "Sistem",
                            Name = "Lokalizasyonlar",
                            Icon = "<path d=\"M21 4H11l-1-3H3c-1.1 0-2 .9-2 2v15c0 1.1.9 2 2 2h8l1 3h9c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zM7 16c-2.76 0-5-2.24-5-5s2.24-5 5-5c1.35 0 2.48.5 3.35 1.3L9.03 8.57c-.38-.36-1.04-.78-2.03-.78-1.74 0-3.15 1.44-3.15 3.21S5.26 14.21 7 14.21c2.01 0 2.84-1.44 2.92-2.41H7v-1.71h4.68c.07.31.12.61.12 1.02C11.8 13.97 9.89 16 7 16zm6.17-5.42h3.7c-.43 1.25-1.11 2.43-2.05 3.47-.31-.35-.6-.72-.86-1.1l-.79-2.37zm8.33 9.92c0 .55-.45 1-1 1H14l2-2.5-1.04-3.1 3.1 3.1.92-.92-3.3-3.25.02-.02c1.13-1.25 1.93-2.69 2.4-4.22H20v-1.3h-4.53V8h-1.29v1.29h-1.44L11.46 5.5h9.04c.55 0 1 .45 1 1v14z\"/><path d=\"M0 0h24v24H0zm0 0h24v24H0z\" fill=\"none\"/>",
                            Path = "/localizations",
                            Rank = 99,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "Studio",
                            Name = "Dinamik Arayüz",
                            Icon = "<g><path d=\"M0,0h24v24H0V0z\" fill=\"none\"/></g><g><path d=\"M19,3h-4.18C14.4,1.84,13.3,1,12,1S9.6,1.84,9.18,3H5C3.9,3,3,3.9,3,5v14c0,1.1,0.9,2,2,2h14c1.1,0,2-0.9,2-2V5 C21,3.9,20.1,3,19,3z M12,2.75c0.41,0,0.75,0.34,0.75,0.75S12.41,4.25,12,4.25s-0.75-0.34-0.75-0.75S11.59,2.75,12,2.75z M9.1,17H7 v-2.14l5.96-5.96l2.12,2.12L9.1,17z M16.85,9.27l-1.06,1.06l-2.12-2.12l1.06-1.06c0.2-0.2,0.51-0.2,0.71,0l1.41,1.41 C17.05,8.76,17.05,9.07,16.85,9.27z\"/></g>",
                            Path = "/ui-configs",
                            Rank = 5,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        },
                        new Menu()
                        {
                            Header = "Studio",
                            Name = "Dinamik Menü",
                            Icon = "<path d=\"M0 0h24v24H0V0z\" fill=\"none\"/><path d=\"M3 18h13v-2H3v2zm0-5h10v-2H3v2zm0-7v2h13V6H3zm18 9.59L17.42 12 21 8.41 19.59 7l-5 5 5 5L21 15.59z\"/>",
                            Path = "/menu-ui-settings",
                            Rank = 6,
                            CreateUser = "SYSTEM",
                            CreateTime = DateTime.Now
                        }
                    };

                    var roleMenus = await roleMenuRepo
                        .Get()
                        .Where(rm => rm.RoleId == role.Id)
                        .ToListAsync();

                    foreach (var menu in menuList)
                    {
                        var menuEntity = await menuRepo.Get().FirstOrDefaultAsync(m => m.Name == menu.Name);

                        if (menuEntity == null)
                        {
                            menuEntity = menu;
                            await menuRepo.AddAsync(menuEntity);
                            await menuRepo.SaveChangesAsync();
                        }
                        
                        if (menuEntity.Id < 1)
                            continue;

                        if (role != null)
                        {
                            if (roleMenus.Any(m => m.MenuId == menuEntity.Id))
                                continue;

                            var roleMenu = new RoleMenu()
                            {
                                MenuId = menuEntity.Id,
                                RoleId = role.Id,
                                CreateUser = "SYSTEM",
                                CreateTime = DateTime.Now
                            };
                            
                            await roleMenuRepo.AddAsync(roleMenu);
                        }
                    }
                    
                    await roleMenuRepo.SaveChangesAsync();
                }
            }
        }
    }
}
