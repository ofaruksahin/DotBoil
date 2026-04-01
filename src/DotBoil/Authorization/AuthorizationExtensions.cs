using System.Security.Claims;

namespace DotBoil;

public static class AuthorizationExtensions
{
    public static bool CheckRole(this ClaimsPrincipal user, string role)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return userRoles.Contains(role);
    }
    
    public static bool CheckAppModule(this ClaimsPrincipal user, string appModule)
    {
        if (user?.Identity?.IsAuthenticated != true) return false;
        
        var appModules = user.Claims
            .Where(c => c.Type == DotBoilClaimTypes.AppModule)
            .Select(c => c.Value)
            .ToList();

        return appModules.Contains(appModule);
    }
}