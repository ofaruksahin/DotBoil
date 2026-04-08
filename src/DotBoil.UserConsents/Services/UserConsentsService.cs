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

        public async Task AcceptConsent(string userId, ConsentType type, string language, CancellationToken cancellationToken = default)
        {
            var consent = await GetConsent(type, language, cancellationToken);

            if (consent is null)
                throw new InvalidOperationException($"Consent not found for type '{type}' and language '{language}'.");

            using var scope = _serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UserConsentsDbContext>();

            var userConsent = new UserConsent
            {
                UserId = userId,
                Type = type,
                Language = language,
                Content = consent.Content,
                Version = consent.Version,
                CreateTime = DateTime.UtcNow,
                CreateUser = userId
            };

            await dbContext.UserConsents.AddAsync(userConsent, cancellationToken);
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
                AcceptedVersion = acceptedVersion
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

        private string BuildCacheKey(ConsentType type, string language)
            => $"{_cachePrefix}{type}:{language}";
    }

    internal sealed class UserConsentCheckResult
    {
        public bool IsUpToDate { get; init; }
        public int CurrentVersion { get; init; }
        public int? AcceptedVersion { get; init; }
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