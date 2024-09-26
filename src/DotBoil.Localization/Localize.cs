using DotBoil.Localization.Configurations;
using DotBoil.Localization.Exceptions;
using DotBoil.Localization.Persistence;
using DotBoil.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DotBoil.Localization
{
    public class Localize : ILocalize
    {
        private string _prefix = "DotBoil:Localization:";
        private IDatabase _cache;
        private ICurrentLanguage _currentLanguage;
        private IServiceProvider _serivceProvider;
        private LocalizationConfiguration _configuration;

        public Localize(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _configuration = scope.ServiceProvider.GetService<LocalizationConfiguration>();
            _cache = ConnectionMultiplexer.Connect(_configuration.Caching.ConnectionString).GetDatabase(0);
            _currentLanguage = scope.ServiceProvider.GetService<ICurrentLanguage>();
            _serivceProvider = serviceProvider;

            Initialize()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public async Task<string> LocalizeText(string name)
        {
            return await LocalizeText(string.Empty, name);
        }

        public async Task<string> LocalizeText(string group, string name)
        {
            try
            {
                var key = string.Concat(_prefix, string.Join(":", _currentLanguage.Language, string.IsNullOrEmpty(group) ? "DotBoil" : group, name));

                TimeSpan? timeSpan = null;

                if (_configuration.Caching.ExpireInHour.HasValue)
                    timeSpan = TimeSpan.FromHours(_configuration.Caching.ExpireInHour.Value);

                var localizedText = await GetOrSetAsync(key, async () =>
                {
                    using var scope = _serivceProvider.CreateAsyncScope();
                    var localizeDbContext = scope.ServiceProvider.GetService<LocalizationDbContext>();

                    return (await localizeDbContext.Localizations.FirstOrDefaultAsync(l =>
                        l.Language == _currentLanguage.Language &&
                        l.Group == group &&
                        l.Key == name))?.Value;
                }, timeSpan);

                if (string.IsNullOrEmpty(localizedText))
                    throw new LocalizeException(_currentLanguage.Language, group, name);

                return localizedText;
            }
            catch (Exception)
            {
                throw new LocalizeException(_currentLanguage.Language, group, name);
            }
        }

        private async Task Initialize()
        {
            using var scope = _serivceProvider.CreateAsyncScope();

            var localizationDbContext = scope.ServiceProvider.GetService<LocalizationDbContext>();
            var localizations = await localizationDbContext.Localizations.ToListAsync();

            foreach (var item in localizations.Where(l => string.IsNullOrEmpty(l.Group)))
                item.Group = "DotBoil";

            var groupedLocalizations = localizations.GroupBy(l => new { l.Language, l.Group });

            TimeSpan? timeSpan = null;

            if (_configuration.Caching.ExpireInHour.HasValue)
                timeSpan = TimeSpan.FromHours(_configuration.Caching.ExpireInHour.Value);

            foreach (var group in groupedLocalizations)
            {
                foreach (var localization in group)
                {
                    var cacheKey = string.Concat(_prefix, string.Join(':', group.Key.Language, group.Key.Group, localization.Key));
                    await _cache.StringSetAsync(cacheKey, await localization.SerializeAsync(), timeSpan);
                }
            }
        }

        private async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> action, TimeSpan? expire = default)
        {
            var cachedValue = await _cache.StringGetAsync(key);
            if (cachedValue.HasValue)
                return (T)Convert.ChangeType(cachedValue.ToString(), typeof(T));

            var result = await action();

            await _cache.StringSetAsync(key, result.ToString(), expire);

            return result;
        }
    }
}
