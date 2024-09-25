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
        private LocalizationConfiguration _configuration;
        private LocalizationDbContext _localizationDbContext;

        public Localize(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _configuration = scope.ServiceProvider.GetService<LocalizationConfiguration>();
            _cache = ConnectionMultiplexer.Connect(_configuration.Caching.ConnectionString).GetDatabase(0);
            _currentLanguage = scope.ServiceProvider.GetService<ICurrentLanguage>();
            _localizationDbContext = scope.ServiceProvider.GetService<LocalizationDbContext>();
        }

        public async Task<string> LocalizeText(string name)
        {
            return await LocalizeText("DotBoil", name);
        }

        public async Task<string> LocalizeText(string group, string name)
        {
            try
            {
                var key = string.Concat(_prefix, string.Join(":", _currentLanguage.Language, group, name));
                var exists = await _cache.KeyExistsAsync(key);

                if (!exists)
                    await Initialize();

                exists = await _cache.KeyExistsAsync(key);

                if (!exists)
                    throw new LocalizeException(_currentLanguage.Language, group, name);

                var localizeJson = await _cache.StringGetAsync(key);

                if (string.IsNullOrEmpty(localizeJson))
                    throw new LocalizeException(_currentLanguage.Language, group, name);

                return (await localizeJson.ToString().DeserializeAsync<Models.Localization>()).Value ??
                    throw new LocalizeException(_currentLanguage.Language, group, name);
            }
            catch (Exception)
            {
                throw new LocalizeException(_currentLanguage.Language, group, name);
            }
        }

        public async Task Initialize()
        {
            var localizations = await _localizationDbContext.Localizations.ToListAsync();

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
    }
}
