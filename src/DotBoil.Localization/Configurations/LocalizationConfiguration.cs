using DotBoil.Configuration;

namespace DotBoil.Localization.Configurations
{
    internal class LocalizationConfiguration : IOptions
    {
        public string Key => "DotBoil:Localization";
        public LocalizationCachingConfiguration Caching { get; set; }
        public LocalizationPersistenceConfiguration Persistence { get; set; }
    }

    internal class LocalizationCachingConfiguration
    {
        public string ConnectionString { get; set; }
        public int? ExpireInHour { get; set; }
    }

    internal class LocalizationPersistenceConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
