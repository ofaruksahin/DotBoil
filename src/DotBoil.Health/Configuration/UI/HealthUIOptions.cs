using DotBoil.Configuration;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIOptions : IOptions
    {
        public string Key => "DotBoil:Health:UI";
        public string Url { get; set; }
        public List<HealthCheckServiceOptions> Services { get; set; } = new();
        public HealthUIMemoryPersistenceOptions InMemory { get; set; }
        public HealthUIMySqlPersistenceOptions MySql { get; set; }
    }
}
