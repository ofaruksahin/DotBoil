using System.Net;
using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Dtos;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.Entities;
using DotBoil.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class ApiEndpointEndpoints
{
    private const string RoutePrefix = "/api/apiendpoint";

    public static IEndpointRouteBuilder MapApiEndpointEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(RoutePrefix)
            .RequireAuthorization()
            .WithTags("ApiEndpoint");

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
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortColumn = null,
        EnumSortDirection sortDirection = EnumSortDirection.Ascending,
        CancellationToken cancellationToken = default)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<ApiEndpoint, DotBoilAuthGuardDbContext>>();
        var appModuleEndpointRepo = serviceProvider.GetRequiredService<IRepository<AppModuleEndpoint, DotBoilAuthGuardDbContext>>();
        var roleApiEndpointRepo = serviceProvider.GetRequiredService<IRepository<RoleApiEndpoint, DotBoilAuthGuardDbContext>>();

        var query = repo.Get();

        var ordered = sortColumn?.ToLowerInvariant() switch
        {
            "controller" => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(e => e.Controller)
                : query.OrderBy(e => e.Controller),
            "action"     => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(e => e.Action)
                : query.OrderBy(e => e.Action),
            _            => query.OrderBy(e => e.Controller).ThenBy(e => e.Action)
        };

        var totalRecords = await ordered.CountAsync(cancellationToken);
        var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var endpoints = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ids = endpoints.Select(e => e.Id).ToList();

        var appModuleMap = await appModuleEndpointRepo.Get()
            .Where(x => ids.Contains(x.ApiEndpointId))
            .GroupBy(x => x.ApiEndpointId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.AppModuleId).ToList(), cancellationToken);

        var roleMap = await roleApiEndpointRepo.Get()
            .Where(x => ids.Contains(x.ApiEndpointId))
            .GroupBy(x => x.ApiEndpointId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleId).ToList(), cancellationToken);

        var items = endpoints.Select(e => new ApiEndpointResponse
        {
            Id           = e.Id,
            Controller   = e.Controller,
            Action       = e.Action,
            AppModuleIds = appModuleMap.TryGetValue(e.Id, out var am) ? am : [],
            RoleIds      = roleMap.TryGetValue(e.Id, out var r) ? r : []
        }).ToList();

        return JsonResponse(BaseResponse.Response(
            new PaginatedModel<ApiEndpointResponse>(pageNumber, pageSize, totalPages, totalRecords, items),
            HttpStatusCode.OK));
    }

    private static async Task<IResult> GetById(
        int id,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<ApiEndpoint, DotBoilAuthGuardDbContext>>();
        var appModuleEndpointRepo = serviceProvider.GetRequiredService<IRepository<AppModuleEndpoint, DotBoilAuthGuardDbContext>>();
        var roleApiEndpointRepo = serviceProvider.GetRequiredService<IRepository<RoleApiEndpoint, DotBoilAuthGuardDbContext>>();

        var endpoint = await repo.Get().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (endpoint is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "ApiEndpoint not found."));

        var appModuleIds = await appModuleEndpointRepo.Get()
            .Where(x => x.ApiEndpointId == id)
            .Select(x => x.AppModuleId)
            .ToListAsync(cancellationToken);

        var roleIds = await roleApiEndpointRepo.Get()
            .Where(x => x.ApiEndpointId == id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        var result = new ApiEndpointResponse
        {
            Id = endpoint.Id,
            Controller = endpoint.Controller,
            Action = endpoint.Action,
            AppModuleIds = appModuleIds,
            RoleIds = roleIds
        };

        return JsonResponse(BaseResponse.Response(result, HttpStatusCode.OK));
    }

    private static async Task<IResult> Create(
        SaveApiEndpointRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Controller))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Controller is required."));

        if (string.IsNullOrWhiteSpace(request.Action))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Action is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<ApiEndpoint, DotBoilAuthGuardDbContext>>();
        var appModuleEndpointRepo = serviceProvider.GetRequiredService<IRepository<AppModuleEndpoint, DotBoilAuthGuardDbContext>>();
        var roleApiEndpointRepo = serviceProvider.GetRequiredService<IRepository<RoleApiEndpoint, DotBoilAuthGuardDbContext>>();

        var exists = await repo.Get().AnyAsync(
            e => e.Controller == request.Controller && e.Action == request.Action,
            cancellationToken);

        if (exists)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "ApiEndpoint already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        var entity = new ApiEndpoint
        {
            Controller = request.Controller.Trim(),
            Action = request.Action.Trim(),
            CreateUser = currentUser,
            CreateTime = DateTime.Now
        };

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        foreach (var appModuleId in request.AppModuleIds)
        {
            await appModuleEndpointRepo.AddAsync(new AppModuleEndpoint
            {
                ApiEndpointId = entity.Id,
                AppModuleId = appModuleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        foreach (var roleId in request.RoleIds)
        {
            await roleApiEndpointRepo.AddAsync(new RoleApiEndpoint
            {
                ApiEndpointId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        if (request.AppModuleIds.Any() || request.RoleIds.Any())
        {
            await appModuleEndpointRepo.SaveChangesAsync();
        }

        var result = new ApiEndpointResponse
        {
            Id = entity.Id,
            Controller = entity.Controller,
            Action = entity.Action,
            AppModuleIds = request.AppModuleIds,
            RoleIds = request.RoleIds
        };

        return JsonResponse(BaseResponse.Response(result, HttpStatusCode.Created, "ApiEndpoint created successfully."));
    }

    private static async Task<IResult> Update(
        int id,
        SaveApiEndpointRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Controller))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Controller is required."));

        if (string.IsNullOrWhiteSpace(request.Action))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Action is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<ApiEndpoint, DotBoilAuthGuardDbContext>>();
        var appModuleEndpointRepo = serviceProvider.GetRequiredService<IRepository<AppModuleEndpoint, DotBoilAuthGuardDbContext>>();
        var roleApiEndpointRepo = serviceProvider.GetRequiredService<IRepository<RoleApiEndpoint, DotBoilAuthGuardDbContext>>();

        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "ApiEndpoint not found."));

        var duplicate = await repo.Get().AnyAsync(
            e => e.Id != id && e.Controller == request.Controller && e.Action == request.Action,
            cancellationToken);

        if (duplicate)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "ApiEndpoint already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        entity.Controller = request.Controller.Trim();
        entity.Action = request.Action.Trim();
        entity.ModifyUser = currentUser;
        entity.UpdateTime = DateTime.Now;

        repo.Update(entity);

        var existingAppModules = await appModuleEndpointRepo.GetAll().AsTracking()
            .Where(x => x.ApiEndpointId == id)
            .ToListAsync(cancellationToken);
        appModuleEndpointRepo.RemoveRange(existingAppModules);

        var existingRoles = await roleApiEndpointRepo.GetAll().AsTracking()
            .Where(x => x.ApiEndpointId == id)
            .ToListAsync(cancellationToken);
        roleApiEndpointRepo.RemoveRange(existingRoles);

        foreach (var appModuleId in request.AppModuleIds)
        {
            await appModuleEndpointRepo.AddAsync(new AppModuleEndpoint
            {
                ApiEndpointId = entity.Id,
                AppModuleId = appModuleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        foreach (var roleId in request.RoleIds)
        {
            await roleApiEndpointRepo.AddAsync(new RoleApiEndpoint
            {
                ApiEndpointId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        await repo.SaveChangesAsync();

        var result = new ApiEndpointResponse
        {
            Id = entity.Id,
            Controller = entity.Controller,
            Action = entity.Action,
            AppModuleIds = request.AppModuleIds,
            RoleIds = request.RoleIds
        };

        return JsonResponse(BaseResponse.Response(result, HttpStatusCode.OK, "ApiEndpoint updated successfully."));
    }

    private static async Task<IResult> Delete(
        int id,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<ApiEndpoint, DotBoilAuthGuardDbContext>>();
        var appModuleEndpointRepo = serviceProvider.GetRequiredService<IRepository<AppModuleEndpoint, DotBoilAuthGuardDbContext>>();
        var roleApiEndpointRepo = serviceProvider.GetRequiredService<IRepository<RoleApiEndpoint, DotBoilAuthGuardDbContext>>();

        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "ApiEndpoint not found."));

        var appModules = await appModuleEndpointRepo.GetAll().AsTracking()
            .Where(x => x.ApiEndpointId == id)
            .ToListAsync(cancellationToken);
        appModuleEndpointRepo.RemoveRange(appModules);

        var roles = await roleApiEndpointRepo.GetAll().AsTracking()
            .Where(x => x.ApiEndpointId == id)
            .ToListAsync(cancellationToken);
        roleApiEndpointRepo.RemoveRange(roles);

        repo.Remove(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "ApiEndpoint deleted successfully."));
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}