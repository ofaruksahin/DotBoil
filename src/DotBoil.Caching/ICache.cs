namespace DotBoil.Caching
{
    public interface ICache
    {
        Task<bool> KeyExistsAsync(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expire = default);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default);
        Task RemoveAsync(string key);
    }
}
