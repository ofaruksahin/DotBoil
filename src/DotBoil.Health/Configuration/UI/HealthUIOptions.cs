using DotBoil.Configuration;

namespace DotBoil.Health.Configuration.UI
{
    internal class HealthUIOptions : IOptions
    {
        public string Key => "DotBoil:Health:UI";
        public string Url { get; set; }
        public List<HealthCheckServiceOptions> Services { get; set; }
        public HealthUIMemoryPersistenceOptions InMemory { get; set; }
        public HealthUISqlServerPersistenceOptions SqlServer { get; set; }
        public HealthUISqLitePersistenceOptions SqLite { get; set; }
        public HealthUIPostgreSQLPersistenceOptions PostgreSQL { get; set; }
        public HealthUIMySqlPersistenceOptions MySql { get; set; }
        public PersistenceType PersistenceType
        {
            get
            {
                if (InMemory is not null)
                    return PersistenceType.InMemory;
                if (SqlServer is not null)
                    return PersistenceType.SqlServer;
                if (SqLite is not null)
                    return PersistenceType.SqLite;
                if (PostgreSQL is not null)
                    return PersistenceType.PostgreSQL;
                if (MySql is not null)
                    return PersistenceType.MySql;

                throw new ArgumentNullException(nameof(PersistenceType));
            }
        }

        public HealthUIOptions()
        {
            Services = new List<HealthCheckServiceOptions>();
        }
    }
}
