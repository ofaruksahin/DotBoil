using DotBoil.Configuration;

namespace DotBoil.UserConsents.Configurations
{
    internal class UserConsentsConfiguration : IOptions
    {
        public string Key => "DotBoil:UserConsents";
        public UserConsentsCachingConfiguration Caching { get; set; }
        public UserConsentsPersistenceConfiguration Persistence { get; set; }
    }

    internal class UserConsentsCachingConfiguration
    {
        public string ConnectionString { get; set; }
        public int? ExpireInHour { get; set; }
    }

    internal class UserConsentsPersistenceConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
