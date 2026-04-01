using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class AuthGuardEndpoints
{
    public static IEndpointRouteBuilder MapAuthGuardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/connect/refresh_token", RefreshToken);

        var appGroup = endpoints
            .MapGroup("/app")
            .RequireAuthorization()
            .WithTags("AuthGuard");

        appGroup.MapPost("/menu", GetMenuList);
        appGroup.MapPost("/permission", CheckPermission);
        appGroup.MapPost("/userinfo", GetUserInfo);
        appGroup.MapPost("/roles", GetCurrentUserRoles);
        appGroup.MapPost("/modules", GetCurrentUserAppModules);

        return endpoints;
    }

    private static async Task<IResult> RefreshToken(
        [FromQuery] string refreshToken,
        IRefreshTokenService refreshTokenService)
    {
        var refreshTokenResult = await refreshTokenService.RefreshToken(refreshToken);

        return Results.Json(
            refreshTokenResult,
            statusCode: refreshTokenResult.IsSuccess
                ? StatusCodes.Status200OK
                : StatusCodes.Status401Unauthorized);
    }

    [Authorize]
    private static async Task<IResult> GetMenuList(IMenuService menuService)
    {
        var menuList = await menuService.GetMenuItems();
        return Results.Ok(menuList);
    }

    [Authorize]
    private static async Task<IResult> CheckPermission(
        [FromQuery] string controller,
        [FromQuery] string action,
        IPermissionService permissionService)
    {
        var hasPermission = await permissionService.CheckPermission(controller, action);

        return hasPermission
            ? Results.Ok()
            : Results.Forbid();
    }

    [Authorize]
    private static async Task<IResult> GetUserInfo(IServiceProvider serviceProvider)
    {
        var userService = serviceProvider.GetRequiredKeyedService<IUserService>("EmailPasswordBasedLogin");
        var getUserInfo = await userService.GetUserInfo();

        return getUserInfo.Claims.Any()
            ? Results.Ok(getUserInfo.Claims)
            : Results.Unauthorized();
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUserRoles(IPermissionService permissionService)
    {
        var roles = await permissionService.GetCurrentUserRoles();

        return Results.Ok(roles.Select(role => new CurrentUserRoleResponse
        {
            Id = role.Id,
            Name = role.Name,
            IsDefault = role.IsDefault
        }));
    }

    [Authorize]
    private static async Task<IResult> GetCurrentUserAppModules(IPermissionService permissionService)
    {
        var appModules = await permissionService.GetCurrentUserAppModules();

        return Results.Ok(appModules.Select(appModule => new CurrentUserAppModuleResponse
        {
            Id = appModule.Id,
            Name = appModule.Name,
            Description = appModule.Description
        }));
    }
}
