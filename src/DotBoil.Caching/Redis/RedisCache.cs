
using DotBoil.Serialization;
using StackExchange.Redis;

namespace DotBoil.Caching.Redis
{
    public class RedisCache : ICache
    {
        private readonly IDatabase _database;

        public RedisCache(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase(0);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default)
        {
            var cachedValue = await _database.StringGetAsync(key);
            if (cachedValue.HasValue)
                return await cachedValue.ToString().DeserializeAsync<T>();

            var result = await action();

            await SetAsync(key, result, expire);

            return result;
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task RemoveAsync(string key)
        {
            await _database.KeyDeleteAsync(key, CommandFlags.FireAndForget);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expire = default)
        {
            var json = await value.SerializeAsync();
            await _database.StringSetAsync(key, json, expire);
        }
    }
}
