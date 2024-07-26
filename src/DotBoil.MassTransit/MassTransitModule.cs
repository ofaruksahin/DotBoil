using DotBoil.Configuration;
using DotBoil.MassTransit.Attributes;
using DotBoil.MassTransit.Configuration;
using DotBoil.MassTransit.Persistence;
using DotBoil.MassTransit.Publishers;
using DotBoil.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotBoil.MassTransit
{
    internal class MassTransitModule : Dependency.Module
    {
        public override async Task<WebApplicationBuilder> AddModule(WebApplicationBuilder builder)
        {
            var persistenceConfiguration = builder.Configuration.GetConfigurations<MassTransitPersistenceConfiguration>();
            var rabbitMqConfiguration = builder.Configuration.GetConfigurations<MassTransitRabbitMqConfiguration>();

            switch (persistenceConfiguration.PersistenceType)
            {
                case MassTransitPersistenceType.MySql:
                    await persistenceConfiguration.MySql.ConfigurePersistence(builder);
                    break;
                default:
                    throw new Exception("Not supported persistence type");
            }

            builder.Services.AddMassTransit(x =>
            {
                var queueConsumerMappings = GetConsumerMappings();

                foreach (var consumerType in queueConsumerMappings.Values.SelectMany(types => types).Distinct())
                    x.AddConsumer(consumerType);

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host($"rabbitmq://{rabbitMqConfiguration.Host}:{rabbitMqConfiguration.Port}", h =>
                    {
                        h.Username(rabbitMqConfiguration.Username);
                        h.Password(rabbitMqConfiguration.Password);
                    });

                    foreach (var mapping in queueConsumerMappings)
                    {
                        cfg.ReceiveEndpoint(mapping.Key, ep =>
                        {
                            foreach (var consumerType in mapping.Value)
                                ep.ConfigureConsumer(context, consumerType);

                            var retryPolicy = rabbitMqConfiguration.GetRetryPolicy(mapping.Key);

                            if (retryPolicy != null)
                            {
                                ep.UseMessageRetry(r => r.Interval(retryPolicy.Interval, TimeSpan.FromSeconds(retryPolicy.RetrySecond)));

                                ep.PrefetchCount = retryPolicy.PrefetchCount;
                                ep.UseConcurrencyLimit(retryPolicy.ConcurrencyLimit);
                            }
                        });
                    }
                });
            });

            builder.Services.AddScoped<MassTransitDbContextSaveChangesInterceptor>();

            builder.Services.AddScoped<IBusPublisher, RabbitMqPublisher>();

            return builder;
        }

        public override Task<WebApplication> UseModule(WebApplication app)
        {
            return Task.FromResult(app);
        }

        private IDictionary<string, List<Type>> GetConsumerMappings()
        {
            var queueConsumerMappings = new Dictionary<string, List<Type>>();

            foreach (var consumerType in AppDomain.CurrentDomain.FindTypesWithInterface(typeof(IConsumer<>)))
            {
                var consumerAttribute = consumerType.GetCustomAttribute(typeof(ConsumerAttribute), true) as ConsumerAttribute;

                if (consumerAttribute == null)
                    continue;

                if (queueConsumerMappings.ContainsKey(consumerAttribute.QueueName))
                {
                    var consumers = queueConsumerMappings[consumerAttribute.QueueName];

                    if (!consumers.Any(consumer => consumer == consumerType))
                        consumers.Add(consumerType);
                }
                else
                {
                    queueConsumerMappings.TryAdd(consumerAttribute.QueueName, new List<Type> { consumerType });
                }
            }

            return queueConsumerMappings;
        }
    }
}
