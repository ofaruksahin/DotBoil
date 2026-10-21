using DotBoil.Entities;

namespace DotBoil.UserConsents.Models
{
    internal class UserConsent : BaseEntity
    {
        public const int IpAddressMaxLength = 45;
        public const int UserAgentMaxLength = 512;

        public string UserId { get; set; }
        public ConsentType Type { get; set; }
        public string Language { get; set; }
        public string Content { get; set; }
        public int Version { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
