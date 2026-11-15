using DotBoil.Entities;

namespace DotBoil.FCM.Models
{
    internal class UserDeviceToken : BaseEntity
    {
        public const int UserIdMaxLength = 100;
        public const int TokenMaxLength = 512;
        public const int DeviceIdMaxLength = 200;

        public string UserId { get; set; }
        public string Token { get; set; }
        public DevicePlatform Platform { get; set; }
        public string DeviceId { get; set; }
        public DateTime LastSeenTime { get; set; }
    }
}
