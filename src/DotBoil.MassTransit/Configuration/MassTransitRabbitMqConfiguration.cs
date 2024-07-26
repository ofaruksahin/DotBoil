using DotBoil.Configuration;

namespace DotBoil.MassTransit.Configuration
{
    internal class MassTransitRabbitMqConfiguration : IOptions
    {
        public string Key => "DotBoil:MessageBroker:MassTransit:RabbitMq";

        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<MassTransitRabbitMqRetryPolicyConfiguration> RetryPolicies { get; set; }

        public MassTransitRabbitMqConfiguration()
        {
            RetryPolicies = new List<MassTransitRabbitMqRetryPolicyConfiguration>();
        }

        public MassTransitRabbitMqRetryPolicyConfiguration GetRetryPolicy(string queueName)
            => RetryPolicies.FirstOrDefault(rp => rp.QueueName == queueName);
    }

    internal class MassTransitRabbitMqRetryPolicyConfiguration
    {
        public string QueueName { get; set; }
        public int Interval { get; set; }
        public int RetrySecond { get; set; }
        public int PrefetchCount { get; set; }
        public int ConcurrencyLimit { get; set; }
    }
}
