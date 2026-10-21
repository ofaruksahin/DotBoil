using DotBoil.Configuration;
using DotBoil.Serialization;
using DotBoil.UserConsents.Configurations;
using DotBoil.UserConsents.Models;
using DotBoil.UserConsents.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DotBoil.UserConsents.Services
{
    internal class UserConsentsService : IUserConsentsService
    {
        private readonly string _cachePrefix;

        private readonly IDatabase _cache;
        private readonly IServiceProvider _serviceProvider;
        private readonly UserConsentsConfiguration _configuration;

        public UserConsentsService(IServiceProvider serviceProvider)
        {
            var scope = serviceProvider.CreateScope();
            _configuration = scope.ServiceProvider.GetRequiredService<UserConsentsConfiguration>();
            _cache = ConnectionMultiplexer.Connect(_configuration.Caching.ConnectionString).GetDatabase(0);
            _serviceProvider = serviceProvider;

            var appConfig = DotBoilApp.Configuration.GetConfigurations<ApplicationConfiguration>();
            _cachePrefix = $"DotBoil:{appConfig.MainApplicationName}:UserConsents:";
        }

        public async Task<ConsentResult> GetConsent(ConsentType type, string language, CancellationToken cancellationToken = default)
        {
            var cacheKey = BuildCacheKey(type, language);
            var cached = await _cache.StringGetAsync(cacheKey);

            if (cached.HasValue)
            {
                return await cached.ToString().DeserializeAsync<ConsentResult>();
            }

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var consent = await dbContext.Consents
                .Where(c => c.Type == type && c.Language == language && !c.IsDeleted)
                .Select(c => new ConsentResult
                {
                    Id = c.Id,
                    Type = c.Type,
                    Language = c.Language,
                    Content = c.Content,
                    IsRequired = c.IsRequired,
                    Version = c.Version
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (consent is null)
                return null;

            await SetCacheAsync(cacheKey, consent);

            return consent;
        }

        public Task AcceptConsent(string userId, ConsentType type, string language, CancellationToken cancellationToken = default)
            => AcceptConsentsInternal(userId, [type], language, null, null, cancellationToken);

        public Task AcceptConsents(string userId, IEnumerable<ConsentType> types, string language, CancellationToken cancellationToken = default)
            => AcceptConsentsInternal(userId, types, language, null, null, cancellationToken);

        public Task AcceptConsent(string userId, ConsentType type, string language, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
            => AcceptConsentsInternal(userId, [type], language, ipAddress, userAgent, cancellationToken);

        public Task AcceptConsents(string userId, IEnumerable<ConsentType> types, string language, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
            => AcceptConsentsInternal(userId, types, language, ipAddress, userAgent, cancellationToken);

        internal async Task AcceptConsentsInternal(
            string userId,
            IEnumerable<ConsentType> types,
            string language,
            string ipAddress,
            string userAgent,
            CancellationToken cancellationToken)
        {
            var typeList = types.Distinct().ToList();

            var consents = await Task.WhenAll(typeList.Select(t => GetConsent(t, language, cancellationToken)));

            var missing = typeList.Zip(consents, (type, consent) => (type, consent))
                .Where(x => x.consent is null)
                .Select(x => x.type.ToString())
                .ToList();

            if (missing.Count == 1 && typeList.Count == 1)
                throw new InvalidOperationException($"Consent not found for type '{missing[0]}' and language '{language}'.");

            if (missing.Count > 0)
                throw new InvalidOperationException($"Consent(s) not found for type(s) '{string.Join(", ", missing)}' and language '{language}'.");

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var now = DateTime.UtcNow;
            ipAddress = Truncate(ipAddress, UserConsent.IpAddressMaxLength);
            userAgent = Truncate(userAgent, UserConsent.UserAgentMaxLength);

            var userConsents = consents.Select(consent => new UserConsent
            {
                UserId = userId,
                Type = consent.Type,
                Language = language,
                Content = consent.Content,
                Version = consent.Version,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CreateTime = now,
                CreateUser = userId
            });

            await dbContext.UserConsents.AddRangeAsync(userConsents, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        internal async Task<Consent> CreateConsentInternal(CreateConsentRequest request, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var consent = new Consent
            {
                Type = request.Type,
                Language = request.Language,
                Content = request.Content,
                IsRequired = request.IsRequired,
                Version = 1,
                CreateTime = DateTime.UtcNow,
                CreateUser = "system"
            };

            await dbContext.Consents.AddAsync(consent, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            await InvalidateCacheAsync(request.Type, request.Language);

            return consent;
        }

        internal async Task<Consent> UpdateConsentInternal(int id, UpdateConsentRequest request, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var existing = await dbContext.Consents
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

            if (existing is null)
                return null;

            var history = new ConsentHistory
            {
                Type = existing.Type,
                Language = existing.Language,
                Content = existing.Content,
                IsRequired = existing.IsRequired,
                Version = existing.Version,
                CreateTime = existing.CreateTime,
                CreateUser = existing.CreateUser,
                UpdateTime = existing.UpdateTime,
                ModifyUser = existing.ModifyUser
            };

            await dbContext.ConsentHistories.AddAsync(history, cancellationToken);

            existing.Content = request.Content;
            existing.IsRequired = request.IsRequired;
            existing.Language = request.Language;
            existing.Version += 1;
            existing.UpdateTime = DateTime.UtcNow;
            existing.ModifyUser = "system";

            await dbContext.SaveChangesAsync(cancellationToken);

            await InvalidateCacheAsync(existing.Type, existing.Language);

            return existing;
        }

        internal async Task<bool> DeleteConsentInternal(int id, CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var consent = await dbContext.Consents
                .AsTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

            if (consent is null)
                return false;

            consent.IsDeleted = true;
            consent.UpdateTime = DateTime.UtcNow;
            consent.ModifyUser = "system";

            await dbContext.SaveChangesAsync(cancellationToken);

            await InvalidateCacheAsync(consent.Type, consent.Language);

            return true;
        }

        internal async Task<UserConsentCheckResult> CheckUserConsentInternal(string userId, ConsentType type, string language, CancellationToken cancellationToken)
        {
            var current = await GetConsent(type, language, cancellationToken);

            if (current is null)
                return new UserConsentCheckResult { IsUpToDate = false, CurrentVersion = 0, AcceptedVersion = null };

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var acceptedVersion = await dbContext.UserConsents
                .Where(uc => uc.UserId == userId && uc.Type == type && uc.Language == language)
                .OrderByDescending(uc => uc.Version)
                .Select(uc => (int?)uc.Version)
                .FirstOrDefaultAsync(cancellationToken);

            return new UserConsentCheckResult
            {
                IsUpToDate = acceptedVersion.HasValue && acceptedVersion.Value >= current.Version,
                CurrentVersion = current.Version,
                AcceptedVersion = acceptedVersion,
                Content = current.Content
            };
        }

        internal async Task<UserConsentsCheckResult> CheckUserConsentsInternal(
            string userId,
            IEnumerable<ConsentType> types,
            string language,
            CancellationToken cancellationToken)
        {
            var typeList = types.Distinct().ToList();

            var currents = await Task.WhenAll(typeList.Select(t => GetConsent(t, language, cancellationToken)));

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var acceptedVersions = await dbContext.UserConsents
                .Where(uc => uc.UserId == userId && uc.Language == language && typeList.Contains(uc.Type))
                .GroupBy(uc => uc.Type)
                .Select(g => new { Type = g.Key, Version = g.Max(uc => uc.Version) })
                .ToDictionaryAsync(x => x.Type, x => x.Version, cancellationToken);

            var items = typeList.Zip(currents, (type, current) =>
            {
                int? acceptedVersion = acceptedVersions.TryGetValue(type, out var version) ? version : null;

                return new UserConsentsCheckItem
                {
                    Type = type,
                    Exists = current is not null,
                    IsRequired = current?.IsRequired ?? false,
                    IsUpToDate = current is not null && acceptedVersion.HasValue && acceptedVersion.Value >= current.Version,
                    CurrentVersion = current?.Version ?? 0,
                    AcceptedVersion = acceptedVersion,
                    Content = current?.Content
                };
            }).ToList();

            return new UserConsentsCheckResult
            {
                IsAllUpToDate = items.All(i => i.IsUpToDate),
                IsAllRequiredUpToDate = items.Where(i => i.IsRequired).All(i => i.IsUpToDate),
                Consents = items
            };
        }

        internal async Task<List<ConsentResult>> GetAllConsentsInternal(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            return await dbContext.Consents
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Language)
                .Select(c => new ConsentResult
                {
                    Id = c.Id,
                    Type = c.Type,
                    Language = c.Language,
                    Content = c.Content,
                    IsRequired = c.IsRequired,
                    Version = c.Version
                })
                .ToListAsync(cancellationToken);
        }

        private async Task SetCacheAsync(string cacheKey, ConsentResult consent)
        {
            TimeSpan? expiry = _configuration.Caching.ExpireInHour.HasValue
                ? TimeSpan.FromHours(_configuration.Caching.ExpireInHour.Value)
                : null;

            var serialized = await consent.SerializeAsync();
            await _cache.StringSetAsync(cacheKey, serialized, expiry, When.Always);
        }

        private async Task InvalidateCacheAsync(ConsentType type, string language)
        {
            var cacheKey = BuildCacheKey(type, language);
            await _cache.KeyDeleteAsync(cacheKey);
        }

        private static string Truncate(string value, int maxLength)
            => string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength];

        private string BuildCacheKey(ConsentType type, string language)
            => $"{_cachePrefix}{type}:{language}";
    }

    internal sealed class UserConsentCheckResult
    {
        public bool IsUpToDate { get; init; }
        public int CurrentVersion { get; init; }
        public int? AcceptedVersion { get; init; }
        public string Content { get; init; }
    }

    internal sealed class UserConsentsCheckResult
    {
        public bool IsAllUpToDate { get; init; }
        public bool IsAllRequiredUpToDate { get; init; }
        public List<UserConsentsCheckItem> Consents { get; init; }
    }

    internal sealed class UserConsentsCheckItem
    {
        public ConsentType Type { get; init; }
        public bool Exists { get; init; }
        public bool IsRequired { get; init; }
        public bool IsUpToDate { get; init; }
        public int CurrentVersion { get; init; }
        public int? AcceptedVersion { get; init; }
        public string Content { get; init; }
    }

    internal sealed class CreateConsentRequest
    {
        public ConsentType Type { get; set; }
        public string Language { get; set; }
        public string Content { get; set; }
        public bool IsRequired { get; set; }
    }

    internal sealed class UpdateConsentRequest
    {
        public string Language { get; set; }
        public string Content { get; set; }
        public bool IsRequired { get; set; }
    }
}