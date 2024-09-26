using DotBoil.Configuration;

namespace DotBoil.Parameter.Configurations
{
    internal class ParameterConfiguration : IOptions
    {
        public string Key => "DotBoil:Parameters";

        public ParameterCachingConfiguration Caching { get; set; }
        public ParameterPersistenceConfiguration Persistence { get; set; }
    }

    internal class ParameterCachingConfiguration
    {
        public string ConnectionString { get; set; }
        public int? ExpireInHour { get; set; }
    }

    internal class ParameterPersistenceConfiguration
    {
        public string ConnectionString { get; set; }
    }
}
