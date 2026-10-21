using StackExchange.Redis;

namespace DotBoil.AuthGuard.Application.Infrastructure.Services;

internal static class UserTokenCacheExtensions
{
    public const string AccessTokenKeyPrefix = "DotBoil:AuthGuard:AccessTokens:";
    public const string RefreshTokenKeyPrefix = "DotBoil:AuthGuard:RefreshTokens:";

    // Redis set holding the access/refresh token cache keys issued to a user,
    // so that all of them can be revoked at once.
    private static string UserTokensKey(int userId) => $"DotBoil:AuthGuard:Users:{userId}:Tokens";

    public static async Task TrackUserToken(this IConnectionMultiplexer redis, int userId, string tokenCacheKey, TimeSpan expiry)
    {
        var database = redis.GetDatabase(0);
        var userTokensKey = UserTokensKey(userId);

        await database.SetAddAsync(userTokensKey, tokenCacheKey);

        // Keep the set alive at least as long as the longest-living token in it.
        var timeToLive = await database.KeyTimeToLiveAsync(userTokensKey);
        if (timeToLive is null || timeToLive < expiry)
            await database.KeyExpireAsync(userTokensKey, expiry);
    }

    public static async Task RevokeUserTokens(this IConnectionMultiplexer redis, int userId)
    {
        var database = redis.GetDatabase(0);
        var userTokensKey = UserTokensKey(userId);

        var tokenCacheKeys = await database.SetMembersAsync(userTokensKey);

        var keys = tokenCacheKeys
            .Select(tokenCacheKey => (RedisKey)tokenCacheKey.ToString())
            .Append(userTokensKey)
            .ToArray();

        await database.KeyDeleteAsync(keys);
    }
}
