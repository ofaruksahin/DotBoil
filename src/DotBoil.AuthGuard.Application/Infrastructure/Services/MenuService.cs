using System.Security.Claims;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

public class MenuService : IMenuService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly ICache _cache;

    public MenuService(
        IHttpContextAccessor httpContextAccessor, 
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        ICache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _userRepository = userRepository;
        _cache = cache;
    }
    
    public async Task<IEnumerable<MenuItem>> GetMenuItems()
    {
        var userIdentifierClaim = _httpContextAccessor.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        if (!int.TryParse(userIdentifierClaim, out var userId))
            return Array.Empty<MenuItem>();

        var cacheKey = $"DotBoil:AuthGuard:Users:{userId}:MenuList";

        return await _cache.GetOrSetAsync(cacheKey, async () =>
        {
            var menuList = await _userRepository
                .Get()
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Roles.Where(r => !r.IsDeleted))
                .SelectMany(r => r.Role.Menus.Where(m => !m.IsDeleted && !m.Menu.IsDeleted).Select(r => r.Menu))
                .ToListAsync();

            menuList = menuList
                .DistinctBy(m => m.Id)
                .ToList();

            return menuList
                .Where(m => m.ParentMenuId is null)
                .Select(m => new MenuItem
                {
                    Name = m.Name,
                    Icon = m.Icon,
                    Path = m.Path,
                    Childs = menuList.Where(mm => mm.ParentMenuId == m.Id)
                        .Select(mm => new MenuItem
                        {
                            Name = mm.Name,
                            Icon = mm.Icon,
                            Path = mm.Path
                        }).ToArray()
                });
        }, TimeSpan.FromDays(1));
    }
}
