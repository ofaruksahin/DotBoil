using System.IdentityModel.Tokens.Jwt;
using DotBoil.EFCore;
using Microsoft.AspNetCore.Http;

namespace DotBoil.Studio.Core.Services;

internal class AuditUser : IAuditUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<string> GetModifierName()
    {
        var email = _httpContextAccessor?
            .HttpContext?
            .User?
            .Claims?
            .FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.PreferredUsername)?.Value;

        return Task.FromResult(email ?? "SYSTEM");
    }
}