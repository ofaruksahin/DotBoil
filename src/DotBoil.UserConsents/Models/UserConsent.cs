using DotBoil.Entities;

namespace DotBoil.UserConsents.Models
{
    internal class UserConsent : BaseEntity
    {
        public string UserId { get; set; }
        public ConsentType Type { get; set; }
        public string Language { get; set; }
        public string Content { get; set; }
        public int Version { get; set; }
    }
}
