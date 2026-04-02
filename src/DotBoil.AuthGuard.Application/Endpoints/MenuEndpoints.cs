using System.Net;
using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Dtos;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class MenuEndpoints
{
    private const string RoutePrefix = "/api/menu";

    public static IEndpointRouteBuilder MapMenuEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(RoutePrefix)
            .RequireAuthorization()
            .WithTags("Menu");

        group.MapGet(string.Empty, GetAll)
            .WithMetadata(new CheckRoleAttribute("Admin"))
            .AddEndpointFilter<CheckRoleEndpointFilter>();

        group.MapGet("/{id:int}", GetById)
            .WithMetadata(new CheckRoleAttribute("Admin"))
            .AddEndpointFilter<CheckRoleEndpointFilter>();

        group.MapPost(string.Empty, Create)
            .WithMetadata(new CheckRoleAttribute("Admin"))
            .AddEndpointFilter<CheckRoleEndpointFilter>();

        group.MapPut("/{id:int}", Update)
            .WithMetadata(new CheckRoleAttribute("Admin"))
            .AddEndpointFilter<CheckRoleEndpointFilter>();

        group.MapDelete("/{id:int}", Delete)
            .WithMetadata(new CheckRoleAttribute("Admin"))
            .AddEndpointFilter<CheckRoleEndpointFilter>();

        return endpoints;
    }

    private static async Task<IResult> GetAll(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<Menu, DotBoilAuthGuardDbContext>>();
        var roleMenuRepo = serviceProvider.GetRequiredService<IRepository<RoleMenu, DotBoilAuthGuardDbContext>>();

        var menus = await repo.Get().ToListAsync(cancellationToken);

        var ids = menus.Select(m => m.Id).ToList();

        var roleMap = await roleMenuRepo.Get()
            .Where(x => ids.Contains(x.MenuId))
            .GroupBy(x => x.MenuId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleId).ToList(), cancellationToken);

        var result = menus.Select(m => new MenuResponse
        {
            Id = m.Id,
            Name = m.Name,
            Icon = m.Icon,
            Path = m.Path,
            ParentMenuId = m.ParentMenuId,
            RoleIds = roleMap.TryGetValue(m.Id, out var r) ? r : []
        });

        return JsonResponse(BaseResponse.Response(result, HttpStatusCode.OK));
    }

    private static async Task<IResult> GetById(
        int id,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<Menu, DotBoilAuthGuardDbContext>>();
        var roleMenuRepo = serviceProvider.GetRequiredService<IRepository<RoleMenu, DotBoilAuthGuardDbContext>>();

        var menu = await repo.Get().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (menu is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Menu not found."));

        var roleIds = await roleMenuRepo.Get()
            .Where(x => x.MenuId == id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        return JsonResponse(BaseResponse.Response(
            new MenuResponse { Id = menu.Id, Name = menu.Name, Icon = menu.Icon, Path = menu.Path, ParentMenuId = menu.ParentMenuId, RoleIds = roleIds },
            HttpStatusCode.OK));
    }

    private static async Task<IResult> Create(
        SaveMenuRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        if (string.IsNullOrWhiteSpace(request.Path))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Path is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<Menu, DotBoilAuthGuardDbContext>>();
        var roleMenuRepo = serviceProvider.GetRequiredService<IRepository<RoleMenu, DotBoilAuthGuardDbContext>>();

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        var entity = new Menu
        {
            Name = request.Name.Trim(),
            Icon = request.Icon ?? string.Empty,
            Path = request.Path.Trim(),
            ParentMenuId = request.ParentMenuId,
            CreateUser = currentUser,
            CreateTime = DateTime.Now
        };

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        foreach (var roleId in request.RoleIds)
        {
            await roleMenuRepo.AddAsync(new RoleMenu
            {
                MenuId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        if (request.RoleIds.Any())
            await roleMenuRepo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new MenuResponse { Id = entity.Id, Name = entity.Name, Icon = entity.Icon, Path = entity.Path, ParentMenuId = entity.ParentMenuId, RoleIds = request.RoleIds },
            HttpStatusCode.Created,
            "Menu created successfully."));
    }

    private static async Task<IResult> Update(
        int id,
        SaveMenuRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        if (string.IsNullOrWhiteSpace(request.Path))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Path is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<Menu, DotBoilAuthGuardDbContext>>();
        var roleMenuRepo = serviceProvider.GetRequiredService<IRepository<RoleMenu, DotBoilAuthGuardDbContext>>();

        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Menu not found."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        entity.Name = request.Name.Trim();
        entity.Icon = request.Icon ?? string.Empty;
        entity.Path = request.Path.Trim();
        entity.ParentMenuId = request.ParentMenuId;
        entity.ModifyUser = currentUser;
        entity.UpdateTime = DateTime.Now;

        repo.Update(entity);

        var existingRoles = await roleMenuRepo.GetAll().AsTracking()
            .Where(x => x.MenuId == id)
            .ToListAsync(cancellationToken);
        roleMenuRepo.RemoveRange(existingRoles);

        foreach (var roleId in request.RoleIds)
        {
            await roleMenuRepo.AddAsync(new RoleMenu
            {
                MenuId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new MenuResponse { Id = entity.Id, Name = entity.Name, Icon = entity.Icon, Path = entity.Path, ParentMenuId = entity.ParentMenuId, RoleIds = request.RoleIds },
            HttpStatusCode.OK,
            "Menu updated successfully."));
    }

    private static async Task<IResult> Delete(
        int id,
        IRepository<Menu, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Menu not found."));

        repo.Remove(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Menu deleted successfully."));
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}