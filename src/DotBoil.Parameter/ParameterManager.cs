using System.Diagnostics;
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

        public async Task<T> GetParameterValue<T>(string section, string name, bool isPublic = false)
        {
            return await GetParameterValue<T>(0, section, name, isPublic);
        }

        public async Task<T> GetParameterValue<T>(string name, bool isPublic = false)
        {
            return await GetParameterValue<T>(0, string.Empty, name, isPublic);
        }

        public async Task<T> GetParameterValue<T>(int tenantId, string name, bool isPublic = false)
        {
            return await GetParameterValue<T>(tenantId, string.Empty, name, isPublic);
        }

        public async Task<T> GetParameterValue<T>(int tenantId, string section, string name, bool isPublic = false)
        {
            var key = string.Join(':', _prefix, tenantId, string.IsNullOrEmpty(section) ? "DotBoil" : section, name, isPublic);
            var timeSpan = default(TimeSpan?);

            if (_configuration.Caching.ExpireInHour.HasValue)
                timeSpan = TimeSpan.FromHours(_configuration.Caching.ExpireInHour.Value);

            return await GetOrSetAsync<T>(key, async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetService<ParameterDbContext>();

                var parameter = await dbContext.Parameters.FirstOrDefaultAsync(p =>
                        p.TenantId == tenantId &&
                        p.Section == section && 
                        p.Key == name &&
                        p.IsPublic == isPublic);
                if (parameter is null)
                    return default(T);

                return (T)Convert.ChangeType(parameter?.Value, typeof(T));
            }, timeSpan);
        }

        private async Task Initialize()
        {
            try
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
                    var key = string.Join(':', _prefix, param.TenantId, param.Section, param.Key, param.IsPublic);
                    await _caching.StringSetAsync(key, param.Value);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default)
        {
            var cachedValue = await _caching.StringGetAsync(key);
            if (cachedValue.HasValue)
                return (T)Convert.ChangeType(cachedValue.ToString(), typeof(T));

            var result = await action();

            await _caching.StringSetAsync(key, result.ToString(), expire, When.Always);

            return result;
        }
    }
}
