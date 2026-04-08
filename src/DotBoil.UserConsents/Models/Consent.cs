using DotBoil.Entities;

namespace DotBoil.UserConsents.Models
{
    internal class Consent : BaseEntity
    {
        public ConsentType Type { get; set; }
        public string Language { get; set; }
        public string Content { get; set; }
        public bool IsRequired { get; set; }
        public int Version { get; set; }
    }
}
