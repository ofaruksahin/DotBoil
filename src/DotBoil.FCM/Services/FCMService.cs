using DotBoil.FCM.Models;
using DotBoil.FCM.Persistence;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.FCM.Services
{
    public class FCMService : IFCMService
    {
        // FCM multicast isteği başına izin verilen maksimum token sayısı
        private const int MulticastBatchSize = 500;

        private readonly FCMDbContext _dbContext;
        private readonly FirebaseMessaging _messaging;

        // FCMDbContext internal olduğu için constructor internal; instance DI üzerinden alınmalıdır
        internal FCMService(FCMDbContext dbContext, FirebaseMessaging messaging)
        {
            _dbContext = dbContext;
            _messaging = messaging;
        }

        public async Task RegisterToken(string userId, string token, DevicePlatform platform, string deviceId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required.", nameof(token));

            token = token.Trim();
            deviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId.Trim();

            if (token.Length > UserDeviceToken.TokenMaxLength)
                throw new ArgumentException($"Token can be at most {UserDeviceToken.TokenMaxLength} characters.", nameof(token));

            if (deviceId?.Length > UserDeviceToken.DeviceIdMaxLength)
                throw new ArgumentException($"DeviceId can be at most {UserDeviceToken.DeviceIdMaxLength} characters.", nameof(deviceId));

            try
            {
                await UpsertToken(userId, token, platform, deviceId, cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Aynı token eş zamanlı olarak iki kez kaydedilmeye çalışıldıysa unique index ihlali oluşur;
                // bu durumda kayıt artık mevcuttur, güncelleme yoluyla bir kez daha dene.
                _dbContext.ChangeTracker.Clear();
                await UpsertToken(userId, token, platform, deviceId, cancellationToken);
            }
        }

        public Task UnregisterToken(string token, CancellationToken cancellationToken = default)
            => UnregisterTokenInternal(null, token, cancellationToken);

        public async Task UnregisterUser(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            await SoftDelete(
                _dbContext.UserDeviceTokens.Where(t => t.UserId == userId && !t.IsDeleted),
                userId,
                cancellationToken);
        }

        public Task<FCMSendResult> SendToUser(string userId, FCMNotification notification,
            CancellationToken cancellationToken = default)
            => SendToUsers([userId], notification, cancellationToken);

        public async Task<FCMSendResult> SendToUsers(IEnumerable<string> userIds, FCMNotification notification,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(notification);

            var userIdList = (userIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (userIdList.Count == 0)
                return new FCMSendResult();

            var tokens = await _dbContext.UserDeviceTokens
                .Where(t => userIdList.Contains(t.UserId) && !t.IsDeleted)
                .Select(t => t.Token)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (tokens.Count == 0)
                return new FCMSendResult();

            var successCount = 0;
            var failureCount = 0;
            var invalidTokens = new List<string>();

            foreach (var batch in tokens.Chunk(MulticastBatchSize))
            {
                var response = await _messaging.SendEachForMulticastAsync(
                    BuildMessage(batch, notification),
                    cancellationToken);

                successCount += response.SuccessCount;
                failureCount += response.FailureCount;

                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (IsInvalidToken(response.Responses[i]))
                        invalidTokens.Add(batch[i]);
                }
            }

            if (invalidTokens.Count > 0)
            {
                await SoftDelete(
                    _dbContext.UserDeviceTokens.Where(t => invalidTokens.Contains(t.Token) && !t.IsDeleted),
                    "system",
                    cancellationToken);
            }

            return new FCMSendResult
            {
                TokenCount = tokens.Count,
                SuccessCount = successCount,
                FailureCount = failureCount,
                RemovedTokenCount = invalidTokens.Count
            };
        }

        internal async Task UnregisterTokenInternal(string userId, string token, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            token = token.Trim();

            var query = _dbContext.UserDeviceTokens.Where(t => t.Token == token && !t.IsDeleted);

            // Kullanıcı bazlı çağrılarda sadece kendi token'ını silebilir
            if (userId is not null)
                query = query.Where(t => t.UserId == userId);

            await SoftDelete(query, userId ?? "system", cancellationToken);
        }

        private async Task UpsertToken(string userId, string token, DevicePlatform platform, string deviceId,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            // Aynı cihazda token yenilendiyse eski token'ları pasifle
            if (deviceId is not null)
            {
                await SoftDelete(
                    _dbContext.UserDeviceTokens.Where(t =>
                        t.UserId == userId && t.DeviceId == deviceId && t.Token != token && !t.IsDeleted),
                    userId,
                    cancellationToken);
            }

            // Token tekildir; başka bir kullanıcıya aitse (aynı cihazda farklı hesapla giriş) yeni kullanıcıya devredilir
            var existing = await _dbContext.UserDeviceTokens
                .AsTracking()
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);

            if (existing is null)
            {
                await _dbContext.UserDeviceTokens.AddAsync(new UserDeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = platform,
                    DeviceId = deviceId,
                    LastSeenTime = now,
                    CreateTime = now,
                    CreateUser = userId
                }, cancellationToken);
            }
            else
            {
                existing.UserId = userId;
                existing.Platform = platform;
                existing.DeviceId = deviceId;
                existing.LastSeenTime = now;
                existing.IsDeleted = false;
                existing.UpdateTime = now;
                existing.ModifyUser = userId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private static Task<int> SoftDelete(IQueryable<UserDeviceToken> query, string modifyUser,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            return query.ExecuteUpdateAsync(setters => setters
                    .SetProperty(t => t.IsDeleted, true)
                    .SetProperty(t => t.UpdateTime, now)
                    .SetProperty(t => t.ModifyUser, modifyUser),
                cancellationToken);
        }

        private static MulticastMessage BuildMessage(IReadOnlyList<string> tokens, FCMNotification notification)
        {
            var hasNotification = !string.IsNullOrWhiteSpace(notification.Title)
                                  || !string.IsNullOrWhiteSpace(notification.Body);

            return new MulticastMessage
            {
                // İstemci SDK'ları (getToken) registration token ürettiği için Fids yerine Tokens kullanılır
#pragma warning disable CS0618
                Tokens = tokens,
#pragma warning restore CS0618
                // Title ve Body boşsa data-only (sessiz) mesaj gönderilir
                Notification = hasNotification
                    ? new Notification
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        ImageUrl = notification.ImageUrl
                    }
                    : null,
                Data = notification.Data
            };
        }

        private static bool IsInvalidToken(SendResponse response)
            => !response.IsSuccess
               && response.Exception?.MessagingErrorCode is MessagingErrorCode.Unregistered
                   or MessagingErrorCode.SenderIdMismatch;
    }
}
