using System.Net;
using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Dtos;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using DotBoil.Entities;
using DotBoil.Enums;
using NETCore.Encrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Endpoints;

internal static class UserEndpoints
{
    private const string RoutePrefix = "/api/user";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup(RoutePrefix)
            .RequireAuthorization()
            .WithTags("User");

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
        IRepository<User, DotBoilAuthGuardDbContext> repo,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortColumn = null,
        EnumSortDirection sortDirection = EnumSortDirection.Ascending,
        CancellationToken cancellationToken = default)
    {
        var query = repo.Get();

        var ordered = sortColumn?.ToLowerInvariant() switch
        {
            "name"     => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(u => u.Name)
                : query.OrderBy(u => u.Name),
            "surname"  => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(u => u.Surname)
                : query.OrderBy(u => u.Surname),
            "username" => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(u => u.Username)
                : query.OrderBy(u => u.Username),
            "email"    => sortDirection == EnumSortDirection.Descending
                ? query.OrderByDescending(u => u.Email)
                : query.OrderBy(u => u.Email),
            _          => query.OrderBy(u => u.Name).ThenBy(u => u.Surname)
        };

        var totalRecords = await ordered.CountAsync(cancellationToken);
        var totalPages   = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var items = await ordered
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserResponse
            {
                Id       = u.Id,
                Provider = u.Provider,
                Name     = u.Name,
                Surname  = u.Surname,
                Username = u.Username,
                Email    = u.Email
            })
            .ToListAsync(cancellationToken);

        return JsonResponse(BaseResponse.Response(
            new PaginatedModel<UserResponse>(pageNumber, pageSize, totalPages, totalRecords, items),
            HttpStatusCode.OK));
    }

    private static async Task<IResult> GetById(
        int id,
        IRepository<User, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var user = await repo.Get()
            .Where(u => u.Id == id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Provider = u.Provider,
                Name = u.Name,
                Surname = u.Surname,
                Username = u.Username,
                Email = u.Email
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        return JsonResponse(BaseResponse.Response(user, HttpStatusCode.OK));
    }

    private static async Task<IResult> Create(
        CreateUserRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Username is required."));

        if (string.IsNullOrWhiteSpace(request.Email))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Email is required."));

        if (string.IsNullOrWhiteSpace(request.Password))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Password is required."));

        var userRepo = serviceProvider.GetRequiredService<IRepository<User, DotBoilAuthGuardDbContext>>();
        var userRoleRepo = serviceProvider.GetRequiredService<IRepository<UserRole, DotBoilAuthGuardDbContext>>();

        var exists = await userRepo.Get().AnyAsync(
            u => u.Username == request.Username.Trim() || u.Email == request.Email.Trim(),
            cancellationToken);

        if (exists)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "User already exists."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        var entity = new User
        {
            Provider = request.Provider?.Trim() ?? string.Empty,
            Name = request.Name?.Trim() ?? string.Empty,
            Surname = request.Surname?.Trim() ?? string.Empty,
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            Password = EncryptProvider.Md5(request.Password),
            CreateUser = currentUser,
            CreateTime = DateTime.Now
        };

        await userRepo.AddAsync(entity);
        await userRepo.SaveChangesAsync();

        foreach (var roleId in request.RoleIds)
        {
            await userRoleRepo.AddAsync(new UserRole
            {
                UserId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        if (request.RoleIds.Any())
            await userRoleRepo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new UserResponse
            {
                Id = entity.Id,
                Provider = entity.Provider,
                Name = entity.Name,
                Surname = entity.Surname,
                Username = entity.Username,
                Email = entity.Email
            },
            HttpStatusCode.Created,
            "User created successfully."));
    }

    private static async Task<IResult> Update(
        int id,
        UpdateUserRequest request,
        HttpContext httpContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Username is required."));

        if (string.IsNullOrWhiteSpace(request.Email))
            return JsonResponse(BaseResponse.Response(HttpStatusCode.BadRequest, "Email is required."));

        var userRepo = serviceProvider.GetRequiredService<IRepository<User, DotBoilAuthGuardDbContext>>();
        var userRoleRepo = serviceProvider.GetRequiredService<IRepository<UserRole, DotBoilAuthGuardDbContext>>();

        var entity = await userRepo.GetAll().AsTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        var duplicate = await userRepo.Get().AnyAsync(
            u => u.Id != id && (u.Username == request.Username.Trim() || u.Email == request.Email.Trim()),
            cancellationToken);

        if (duplicate)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.Conflict, "Username or email already in use."));

        var currentUser = httpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "ADMIN";

        entity.Name = request.Name?.Trim() ?? string.Empty;
        entity.Surname = request.Surname?.Trim() ?? string.Empty;
        entity.Username = request.Username.Trim();
        entity.Email = request.Email.Trim();
        entity.ModifyUser = currentUser;
        entity.UpdateTime = DateTime.Now;

        if (!string.IsNullOrWhiteSpace(request.Password))
            entity.Password = EncryptProvider.Md5(request.Password);

        userRepo.Update(entity);

        var existingRoles = await userRoleRepo.GetAll().AsTracking()
            .Where(x => x.UserId == id)
            .ToListAsync(cancellationToken);
        userRoleRepo.RemoveRange(existingRoles);

        foreach (var roleId in request.RoleIds)
        {
            await userRoleRepo.AddAsync(new UserRole
            {
                UserId = entity.Id,
                RoleId = roleId,
                CreateUser = currentUser,
                CreateTime = DateTime.Now
            });
        }

        await userRepo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(
            new UserResponse
            {
                Id = entity.Id,
                Provider = entity.Provider,
                Name = entity.Name,
                Surname = entity.Surname,
                Username = entity.Username,
                Email = entity.Email
            },
            HttpStatusCode.OK,
            "User updated successfully."));
    }

    private static async Task<IResult> Delete(
        int id,
        IRepository<User, DotBoilAuthGuardDbContext> repo,
        CancellationToken cancellationToken)
    {
        var entity = await repo.GetAll().AsTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (entity is null)
            return JsonResponse(BaseResponse.Response(HttpStatusCode.NotFound, "User not found."));

        repo.Remove(entity);
        await repo.SaveChangesAsync();

        return JsonResponse(BaseResponse.Response(HttpStatusCode.OK, "User deleted successfully."));
    }

    private static IResult JsonResponse(BaseResponse response)
        => Results.Json(response, statusCode: (int)response.StatusCode);
}