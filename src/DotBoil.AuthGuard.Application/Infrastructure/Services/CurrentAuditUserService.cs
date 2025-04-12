using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotBoil.EFCore;
using Microsoft.AspNetCore.Http;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal class CurrentAuditUserService : IAuditUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentAuditUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public async Task<string> GetModifierName()
    {
        return _httpContextAccessor.HttpContext.User.Claims?.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PreferredUsername)?.Value ?? string.Empty;
    }
}