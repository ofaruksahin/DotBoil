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

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class RoleEndpoints
{
    private const string RoutePrefix = "/api/role";

    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(RoutePrefix)
            .RequireAuthorization()
            .WithTags("Role");

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
        IRepository<Role, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var roles = await repo.Get()
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                IsDefault = r.IsDefault
            })
            .ToListAsync(cancellationToken);

        return JsonResponse(BaseResponse.Response(roles, HttpStatusCode.OK));
    }

    private static async Task<IResult> GetById(
        int id,
        IRepository<Role, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var role = await repo.Get()
            .Where(r => r.Id == id)
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                IsDefault = r.IsDefault
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Role not found."));

        return JsonResponse(BaseResponse.Response(role, HttpStatusCode.OK));
    }

    private static async Task<IResult> Create(
        SaveRoleRequest request,
        HttpContext httpContext,
        IRepository<Role, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        var exists = await repo.Get().AnyAsync(r => r.Name == request.Name.Trim(), cancellationToken);

        if (exists)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Role already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        var entity = new Role
        {
            Name = request.Name.Trim(),
            IsDefault = request.IsDefault,
            CreateUser = currentUser,
            CreateTime = DateTime.Now
        };

        await repo.AddAsync(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new RoleResponse { Id = entity.Id, Name = entity.Name, IsDefault = entity.IsDefault },
            HttpStatusCode.Created,
            "Role created successfully."));
    }

    private static async Task<IResult> Update(
        int id,
        SaveRoleRequest request,
        HttpContext httpContext,
        IRepository<Role, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Name is required."));

        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Role not found."));

        var duplicate = await repo.Get().AnyAsync(
            r => r.Id != id && r.Name == request.Name.Trim(),
            cancellationToken);

        if (duplicate)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Role already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        entity.Name = request.Name.Trim();
        entity.IsDefault = request.IsDefault;
        entity.ModifyUser = currentUser;
        entity.UpdateTime = DateTime.Now;

        repo.Update(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new RoleResponse { Id = entity.Id, Name = entity.Name, IsDefault = entity.IsDefault },
            HttpStatusCode.OK,
            "Role updated successfully."));
    }

    private static async Task<IResult> Delete(
        int id,
        IRepository<Role, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "Role not found."));

        repo.Remove(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "Role deleted successfully."));
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}