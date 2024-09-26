using DotBoil.Parameter.Configurations;
using DotBoil.Parameter.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DotBoil.Parameter
{
    internal class ParameterManager : IParameterManager
    {
        private IDatabase _caching;
        private IServiceProvider _serviceProvider;
        private ParameterConfiguration _configuration;

        private string _prefix = "DotBoil:Parameters";

        public ParameterManager(IServiceProvider serviceProvider, ParameterConfiguration configuration)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;

            Initialize()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public async Task<T> GetParameterValue<T>(string section, string name)
        {
            var key = string.Join(':', _prefix, string.IsNullOrEmpty(section) ? "DotBoil" : section, name);
            var timeSpan = default(TimeSpan?);

            if (_configuration.Caching.ExpireInHour.HasValue)
                timeSpan = TimeSpan.FromHours(_configuration.Caching.ExpireInHour.Value);

            return await GetOrSetAsync<T>(key, async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ParameterDbContext>();

                var parameter = await dbContext.Parameters.FirstOrDefaultAsync(p => p.Section == section && p.Key == name);
                if (parameter is null)
                    return default(T);

                return (T)Convert.ChangeType(parameter?.Value, typeof(T));
            }, timeSpan);
        }

        public async Task<T> GetParameterValue<T>(string name)
        {
            return await GetParameterValue<T>(string.Empty, name);
        }

        private async Task Initialize()
        {
            _caching = (await ConnectionMultiplexer.ConnectAsync(_configuration.Caching.ConnectionString))?.GetDatabase(0);

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetService<ParameterDbContext>();

            var parameters = await dbContext.Parameters.ToListAsync();

            foreach (var param in parameters.Where(p => string.IsNullOrEmpty(p.Section)))
            {
                param.Section = "DotBoil";
            }

            foreach (var param in parameters)
            {
                var key = string.Join(':', _prefix, param.Section, param.Key);
                await _caching.StringSetAsync(key, param.Value);
            }
        }

        private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default)
        {
            var cachedValue = await _caching.StringGetAsync(key);
            if (cachedValue.HasValue)
                return (T)Convert.ChangeType(cachedValue.ToString(), typeof(T));

            var result = await action();

            await _caching.StringSetAsync(key, result.ToString(), expire);

            return result;
        }
    }
}
