using DotBoil.FCM.Models;

namespace DotBoil.FCM
{
    public interface IFCMService
    {
        Task RegisterToken(string userId, string token, DevicePlatform platform, string deviceId = null,
            CancellationToken cancellationToken = default);

        Task UnregisterToken(string token, CancellationToken cancellationToken = default);

        Task UnregisterUser(string userId, CancellationToken cancellationToken = default);

        Task<FCMSendResult> SendToUser(string userId, FCMNotification notification,
            CancellationToken cancellationToken = default);

        Task<FCMSendResult> SendToUsers(IEnumerable<string> userIds, FCMNotification notification,
            CancellationToken cancellationToken = default);
    }

    public sealed class FCMNotification
    {
        public string Title { get; init; }
        public string Body { get; init; }
        public string ImageUrl { get; init; }
        public IReadOnlyDictionary<string, string> Data { get; init; }
    }

    public sealed class FCMSendResult
    {
        public int TokenCount { get; init; }
        public int SuccessCount { get; init; }
        public int FailureCount { get; init; }
        public int RemovedTokenCount { get; init; }
    }
}
