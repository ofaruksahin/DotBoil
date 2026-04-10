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

internal static class AppModuleEndpoints
{
    private const string RoutePrefix = "/api/appmodule";

    public static IEndpointRouteBuilder MapAppModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(RoutePrefix)
            .RequireAuthorization()
            .WithTags("AppModule");

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
        int pageNumber = 1,
        int pageSize = 10,
        string? sortColumn = null,
        EnumSortDirection sortDirection = EnumSortDirection.Ascending,
        CancellationToken cancellationToken = default)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<AppModule, DotBoilAuthGuardDbContext>>();
        var roleAppModuleRepo = serviceProvider.GetRequiredService<IRepository<RoleAppModule, DotBoilAuthGuardDbContext>>();

        var query = repo.Get();

        var ordered = sortColumn?.ToLowerInvariant() switch
        {
            "name" => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(m => m.Name)
                : query.OrderBy(m => m.Name),
            _      => query.OrderBy(m => m.Name)
        };

        var totalRecords = await ordered.CountAsync(cancellationToken);
        var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var modules = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var ids = modules.Select(m => m.Id).ToList();

        var roleMap = await roleAppModuleRepo.Get()
            .Where(x => ids.Contains(x.AppModuleId))
            .GroupBy(x => x.AppModuleId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.RoleId).ToList(), cancellationToken);

        var items = modules.Select(m => new AppModuleResponse
        {
            Id          = m.Id,
            Name        = m.Name,
            Description = m.Description,
            RoleIds     = roleMap.TryGetValue(m.Id, out var r) ? r : []
        }).ToList();

        return JsonResponse(BaseResponse.Response(
            new PaginatedModel<AppModuleResponse>(pageNumber, pageSize, totalPages, totalRecords, items),
            HttpStatusCode.OK));
    }

    private static async Task<IResult> GetById(
        int id,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var repo = serviceProvider.GetRequiredService<IRepository<AppModule, DotBoilAuthGuardDbContext>>();
        var roleAppModuleRepo = serviceProvider.GetRequiredService<IRepository<RoleAppModule, DotBoilAuthGuardDbContext>>();

        var module = await repo.Get().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (module is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "AppModule not found."));

        var roleIds = await roleAppModuleRepo.Get()
            .Where(x => x.AppModuleId == id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        return JsonResponse(BaseResponse.Response(
            new AppModuleResponse { Id = module.Id, Name = module.Name, Description = module.Description, RoleIds = roleIds },
            HttpStatusCode.OK));
    }

    private static async Task<IResult> Create(
        SaveAppModuleRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<AppModule, DotBoilAuthGuardDbContext>>();
        var roleAppModuleRepo = serviceProvider.GetRequiredService<IRepository<RoleAppModule, DotBoilAuthGuardDbContext>>();

        var exists = await repo.Get().AnyAsync(m => m.Name == request.Name.Trim(), cancellationToken);

        if (exists)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "AppModule already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        var entity = new AppModule
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            CreateUser = currentUser,
            CreateTime = DateTime.Now
        };

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        foreach (var roleId in request.RoleIds)
        {
            await roleAppModuleRepo.AddAsync(new RoleAppModule
            {
                AppModuleId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        if (request.RoleIds.Any())
            await roleAppModuleRepo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new AppModuleResponse { Id = entity.Id, Name = entity.Name, Description = entity.Description, RoleIds = request.RoleIds },
            HttpStatusCode.Created,
            "AppModule created successfully."));
    }

    private static async Task<IResult> Update(
        int id,
        SaveAppModuleRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        var repo = serviceProvider.GetRequiredService<IRepository<AppModule, DotBoilAuthGuardDbContext>>();
        var roleAppModuleRepo = serviceProvider.GetRequiredService<IRepository<RoleAppModule, DotBoilAuthGuardDbContext>>();

        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "AppModule not found."));

        var duplicate = await repo.Get().AnyAsync(
            m => m.Id != id && m.Name == request.Name.Trim(),
            cancellationToken);

        if (duplicate)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "AppModule already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        entity.Name = request.Name.Trim();
        entity.Description = request.Description.Trim();
        entity.ModifyUser = currentUser;
        entity.UpdateTime = DateTime.Now;

        repo.Update(entity);

        var existingRoles = await roleAppModuleRepo.GetAll().AsTracking()
            .Where(x => x.AppModuleId == id)
            .ToListAsync(cancellationToken);
        roleAppModuleRepo.RemoveRange(existingRoles);

        foreach (var roleId in request.RoleIds)
        {
            await roleAppModuleRepo.AddAsync(new RoleAppModule
            {
                AppModuleId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new AppModuleResponse { Id = entity.Id, Name = entity.Name, Description = entity.Description, RoleIds = request.RoleIds },
            HttpStatusCode.OK,
            "AppModule updated successfully."));
    }

    private static async Task<IResult> Delete(
        int id,
        IRepository<AppModule, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "AppModule not found."));

        repo.Remove(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "AppModule deleted successfully."));
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}