using DotBoil.Configuration;

namespace DotBoil.MassTransit.Configurations
{
    internal class MassTransitOptions : IOptions
    {
        public string Key => "DotBoil:MassTransit";

        public MessageBrokers MessageBrokerType { get; set; }

        public RabbitMqOptions RabbitMq { get; set; }
        public PersistenceType PersistenceType { get; set; }
        public MySqlOptions MySql { get; set; }
    }
}
