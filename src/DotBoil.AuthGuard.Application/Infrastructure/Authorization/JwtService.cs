using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotBoil.AuthGuard.Application.Infrastructure.Authorization;

internal sealed class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;
    private readonly ITokenPermissionService _tokenPermissionService;

    public JwtService(JwtOptions jwtOptions, ITokenPermissionService tokenPermissionService)
    {
        _jwtOptions = jwtOptions;
        _tokenPermissionService = tokenPermissionService;
    }
    
    public async Task<string> GenerateToken(IEnumerable<Claim> claims, int expirationMinutes)
    {
        var claimList = claims
            .Where(claim =>
                claim.Type != ClaimTypes.Role &&
                claim.Type != DotBoilClaimTypes.AppModule)
            .ToList();

        if (TryGetUserId(claimList, out var userId))
        {
            var roles = await _tokenPermissionService.GetCurrentUserRoles(userId);
            var appModules = await _tokenPermissionService.GetCurrentUserAppModules(userId);

            foreach (var roleName in roles
                         .Select(role => role.Name)
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                if (claimList.Any(claim =>
                        claim.Type == ClaimTypes.Role &&
                        claim.Value.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                claimList.Add(new Claim(ClaimTypes.Role, roleName));
            }

            foreach (var appModuleName in appModules
                         .Select(appModule => appModule.Name)
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct(StringComparer.InvariantCultureIgnoreCase))
            {
                if (claimList.Any(claim =>
                        claim.Type == DotBoilClaimTypes.AppModule &&
                        claim.Value.Equals(appModuleName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                claimList.Add(new Claim(DotBoilClaimTypes.AppModule, appModuleName));
            }
        }

        var secretKey = _jwtOptions.SecretKey;
        var issuer = _jwtOptions.Issuer;
        var audience = _jwtOptions.Audience;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claimList,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool TryGetUserId(IEnumerable<Claim> claims, out int userId)
    {
        var userIdClaim = claims.FirstOrDefault(claim =>
            claim.Type == JwtRegisteredClaimNames.Sub)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }
}
