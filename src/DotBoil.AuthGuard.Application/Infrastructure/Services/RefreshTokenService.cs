using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotBoil;
using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Domain.Interfaces;
using DotBoil.AuthGuard.Application.Domain.ValueObjects;
using DotBoil.AuthGuard.Application.Infrastructure.Authorization.Options;
using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.Caching;
using DotBoil.EFCore;
using DotBoil.Localization;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IJwtService _jwtService;
    private readonly ITokenPermissionService _tokenPermissionService;
    private readonly JwtOptions _jwtOptions;
    private readonly ICache _cache;
    private readonly ILocalize _localize;
    private readonly IRepository<User, DotBoilAuthGuardDbContext> _userRepository;
    private readonly IConnectionMultiplexer _redis;

    public RefreshTokenService(
        IJwtService jwtService,
        ITokenPermissionService tokenPermissionService,
        JwtOptions jwtOptions,
        ICache cache,
        ILocalize localize,
        IRepository<User, DotBoilAuthGuardDbContext> userRepository,
        IConnectionMultiplexer redis)
    {
        _jwtService = jwtService;
        _tokenPermissionService = tokenPermissionService;
        _jwtOptions = jwtOptions;
        _cache = cache;
        _localize = localize;
        _userRepository = userRepository;
        _redis = redis;
    }

    public async Task<RefreshTokenResult> RefreshToken(string refreshToken)
    {
        var refreshTokenExists = await _cache.KeyExistsAsync($"DotBoil:AuthGuard:RefreshTokens:{refreshToken}");

        if (!refreshTokenExists)
            return RefreshTokenResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        var claimsDictionary = await _cache.GetOrSetAsync(
            $"DotBoil:AuthGuard:RefreshTokens:{refreshToken}",
            async () => new Dictionary<string, string>(),
            TimeSpan.FromSeconds(5));

        if (!claimsDictionary.Any())
            return RefreshTokenResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));

        var claims = claimsDictionary.Select(claim => new Claim(claim.Key, claim.Value)).ToList();

        if (TryGetUserId(claims, out var claimUserId) &&
            await _userRepository.Get().AnyAsync(u => u.Id == claimUserId && u.IsPassive))
        {
            await _cache.RemoveAsync($"DotBoil:AuthGuard:RefreshTokens:{refreshToken}");
            return RefreshTokenResult.Failure(await _localize.LocalizeText("Login", "AuthorizationFailed"));
        }

        var refreshedClaims = await RefreshPermissionClaims(claims);
        var refreshedClaimsDictionary = BuildCacheClaimsDictionary(refreshedClaims);

        var createdAccessToken = await _jwtService.GenerateToken(refreshedClaims, _jwtOptions.AccessTokenExpirationMinutes);
        var createdAccessTokenExpiryTime = DateTime.Now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);

        await _cache.SetAsync(
            $"DotBoil:AuthGuard:AccessTokens:{createdAccessToken}",
            refreshedClaimsDictionary,
            TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));

        if (TryGetUserId(refreshedClaims, out var userId))
        {
            await _redis.TrackUserToken(
                userId,
                $"{UserTokenCacheExtensions.AccessTokenKeyPrefix}{createdAccessToken}",
                TimeSpan.FromMinutes(_jwtOptions.AccessTokenExpirationMinutes));

            await _userRepository.UpdateLastLoginDate(userId);
        }

        return RefreshTokenResult.Success(createdAccessToken, createdAccessTokenExpiryTime);
    }

    private async Task<List<Claim>> RefreshPermissionClaims(IEnumerable<Claim> claims)
    {
        var refreshedClaims = claims
            .Where(claim =>
                claim.Type != ClaimTypes.Role &&
                claim.Type != DotBoilClaimTypes.AppModule)
            .ToList();

        if (!TryGetUserId(refreshedClaims, out var userId))
            return refreshedClaims;

        var roles = await _tokenPermissionService.GetCurrentUserRoles(userId);
        var appModules = await _tokenPermissionService.GetCurrentUserAppModules(userId);

        refreshedClaims.AddRange(roles
            .Select(role => role.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Select(roleName => new Claim(ClaimTypes.Role, roleName)));

        refreshedClaims.AddRange(appModules
            .Select(appModule => appModule.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Select(appModuleName => new Claim(DotBoilClaimTypes.AppModule, appModuleName)));

        return refreshedClaims;
    }

    private static bool TryGetUserId(IEnumerable<Claim> claims, out int userId)
    {
        var userIdClaim = claims.FirstOrDefault(claim =>
            claim.Type == JwtRegisteredClaimNames.Sub ||
            claim.Type == ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out userId);
    }

    private static Dictionary<string, string> BuildCacheClaimsDictionary(IEnumerable<Claim> claims)
    {
        return claims
            .Where(claim =>
                claim.Type != ClaimTypes.Role &&
                claim.Type != DotBoilClaimTypes.AppModule)
            .ToDictionary(claim => claim.Type, claim => claim.Value);
    }
}
