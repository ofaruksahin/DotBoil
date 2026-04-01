using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class PermissionService : IPermissionService, ITokenPermissionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly ICache _cache;

    public PermissionService(
        IHttpContextAccessor httpContextAccessor,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        ICache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<bool> CheckPermission(string controller, string action)
    {
        var userIdentifierClaim = _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        if (!int.TryParse(userIdentifierClaim, out var userId))
            return false;

        var cacheKey = $"DotBoil:AuthGuard:Users:{userId}:Permissions";

        var permissionList = await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            return _userRepository
                .Get()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Roles.Where(r => !r.IsDeleted))
                .SelectMany(r =>
                    r.Role.AppModules
                        .Where(am => !am.IsDeleted)
                        .SelectMany(am => am.AppModule.ApiEndpoints.Where(ae => !ae.IsDeleted).Select(ae => ae.ApiEndpoint))
                    .Concat(
                        r.Role.ApiEndpoints.Where(ae => !ae.IsDeleted).Select(ae => ae.ApiEndpoint)))
                .ToList();
        }, TimeSpan.FromDays(1));
        
        return permissionList.Any(p => 
            p.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase) &&
            p.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task<IEnumerable<Role>> GetCurrentUserRoles()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Array.Empty<Role>();

        return await GetCurrentUserRoles(userId);
    }

    public async Task<IEnumerable<AppModule>> GetCurrentUserAppModules()
    {
        if (!TryGetCurrentUserId(out var userId))
            return Array.Empty<AppModule>();

        return await GetCurrentUserAppModules(userId);
    }

    Task<IEnumerable<Role>> ITokenPermissionService.GetCurrentUserRoles(int userId)
    {
        return GetCurrentUserRoles(userId);
    }

    Task<IEnumerable<AppModule>> ITokenPermissionService.GetCurrentUserAppModules(int userId)
    {
        return GetCurrentUserAppModules(userId);
    }

    private async Task<IEnumerable<Role>> GetCurrentUserRoles(int userId)
    {
        var cacheKey = $"DotBoil:AuthGuard:Users:{userId}:Roles";

        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var roleList = await _userRepository
                .Get()
                .Where(user => user.Id == userId)
                .SelectMany(user => user.Roles.Where(userRole =>
                    !userRole.IsDeleted &&
                    !userRole.Role.IsDeleted))
                .Select(userRole => new
                {
                    userRole.Role.Id,
                    userRole.Role.Name,
                    userRole.Role.IsDefault,
                    userRole.Role.CreateUser,
                    userRole.Role.ModifyUser,
                    userRole.Role.CreateTime,
                    userRole.Role.UpdateTime,
                    userRole.Role.IsDeleted
                })
                .Distinct()
                .OrderBy(role => role.Name)
                .ToListAsync();

            return roleList.Select(role => new Role
            {
                Id = role.Id,
                Name = role.Name,
                IsDefault = role.IsDefault,
                CreateUser = role.CreateUser,
                ModifyUser = role.ModifyUser,
                CreateTime = role.CreateTime,
                UpdateTime = role.UpdateTime,
                IsDeleted = role.IsDeleted,
                Users = new List<UserRole>(),
                Menus = new List<RoleMenu>(),
                AppModules = new List<RoleAppModule>(),
                ApiEndpoints = new List<RoleApiEndpoint>()
            });
        }, TimeSpan.FromDays(1));
    }

    private async Task<IEnumerable<AppModule>> GetCurrentUserAppModules(int userId)
    {
        var cacheKey = $"DotBoil:AuthGuard:Users:{userId}:AppModules";

        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var appModuleList = await _userRepository
                .Get()
                .Where(user => user.Id == userId)
                .SelectMany(user => user.Roles.Where(userRole =>
                    !userRole.IsDeleted &&
                    !userRole.Role.IsDeleted))
                .SelectMany(userRole => userRole.Role.AppModules.Where(roleAppModule =>
                    !roleAppModule.IsDeleted &&
                    !roleAppModule.AppModule.IsDeleted))
                .Select(roleAppModule => new
                {
                    roleAppModule.AppModule.Id,
                    roleAppModule.AppModule.Name,
                    roleAppModule.AppModule.Description,
                    roleAppModule.AppModule.CreateUser,
                    roleAppModule.AppModule.ModifyUser,
                    roleAppModule.AppModule.CreateTime,
                    roleAppModule.AppModule.UpdateTime,
                    roleAppModule.AppModule.IsDeleted
                })
                .Distinct()
                .OrderBy(appModule => appModule.Name)
                .ToListAsync();

            return appModuleList.Select(appModule => new AppModule
            {
                Id = appModule.Id,
                Name = appModule.Name,
                Description = appModule.Description,
                CreateUser = appModule.CreateUser,
                ModifyUser = appModule.ModifyUser,
                CreateTime = appModule.CreateTime,
                UpdateTime = appModule.UpdateTime,
                IsDeleted = appModule.IsDeleted,
                ApiEndpoints = new List<AppModuleEndpoint>(),
                Roles = new List<RoleAppModule>()
            });
        }, TimeSpan.FromDays(1));
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var userIdentifierClaim = _httpContextAccessor.HttpContext?.User?.Claims
            .FirstOrDefault(claim =>
                claim.Type == ClaimTypes.NameIdentifier ||
                claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(userIdentifierClaim, out userId);
    }
}
