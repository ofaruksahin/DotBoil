using DotBoil.Configuration;

namespace DotBoil.MassTransit.Configuration
{
    internal class MassTransitPersistenceConfiguration : IOptions
    {
        public string Key => "DotBoil:MessageBroker:MassTransit:Persistence";

        public string ConnectionString { get; set; }
    }
}
