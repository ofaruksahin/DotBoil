using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class PermissionService : IPermissionService
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
                    r.AppModules
                        .Where(am => !am.IsDeleted)
                        .SelectMany(am => am.ApiEndpoints.Where(ae => !ae.IsDeleted))
                    .Concat(
                        r.ApiEndpoints.Where(ae => !ae.IsDeleted)))
                .ToList();
        }, TimeSpan.FromDays(1));
        
        return permissionList.Any(p => 
            p.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase) &&
            p.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
    }
}
