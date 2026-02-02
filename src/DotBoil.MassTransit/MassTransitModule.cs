using DotBoil.Configuration;
using DotBoil.MassTransit.Attributes;
using DotBoil.MassTransit.Configuration;
using DotBoil.MassTransit.Persistence;
using DotBoil.MassTransit.Publishers;
using DotBoil.Reflection;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit
{
    internal class MassTransitModule : Dependency.Module
    {
        public override string Name => "MassTransit";
        public override IEnumerable<string> DependsOn { get; } = Enumerable.Empty<string>();
        public override int Order { get; } = 999;

        public override async Task AddModule()
        {
            var persistenceConfiguration = DotBoilApp.Configuration.GetConfigurations<MassTransitPersistenceConfiguration>();
            var rabbitMqConfiguration = DotBoilApp.Configuration.GetConfigurations<MassTransitRabbitMqConfiguration>();

            switch (persistenceConfiguration.PersistenceType)
            {
                case MassTransitPersistenceType.MySql:
                    await persistenceConfiguration.MySql.ConfigurePersistence();
                    break;
                default:
                    throw new Exception("Not supported persistence type");
            }

            DotBoilApp.Services.AddMassTransit(x =>
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
                    
                    cfg.ConfigureJsonSerializerOptions(jsonSerializerOptions =>
                    {
                        jsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                        jsonSerializerOptions.WriteIndented = true;

                        return jsonSerializerOptions;
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

            DotBoilApp.Services.TryAddScoped<MassTransitDbContextSaveChangesInterceptor>();

            DotBoilApp.Services.TryAddScoped<IBusPublisher, RabbitMqPublisher>();
        }

        public override async Task UseModule()
        {
            var scope = DotBoilApp.Host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MassTransitDbContext>();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private IDictionary<string, List<Type>> GetConsumerMappings()
        {
            var rabbitMqConfiguration = DotBoilApp.Configuration.GetConfigurations<MassTransitRabbitMqConfiguration>();
            var queueConsumerMappings = new Dictionary<string, List<Type>>();
            
            var applicationConfiguration = DotBoilApp.Configuration.GetConfigurations<ApplicationConfiguration>();

            foreach (var consumerType in AppDomain.CurrentDomain.FindTypesWithInterface(typeof(IConsumer<>)))
            {
                if (!rabbitMqConfiguration.Consumers.Any(c => c.StartsWith(consumerType.FullName)))
                    continue;
                    
                var consumerAttribute = consumerType.GetCustomAttribute(typeof(ConsumerAttribute), true) as ConsumerAttribute;

                if (consumerAttribute == null)
                    continue;

                var queueName = $"{applicationConfiguration.MainApplicationName}_{consumerAttribute.QueueName}";

                if (queueConsumerMappings.ContainsKey(queueName))
                {
                    var consumers = queueConsumerMappings[queueName];

                    if (!consumers.Any(consumer => consumer == consumerType))
                        consumers.Add(consumerType);
                }
                else
                {
                    queueConsumerMappings.TryAdd(queueName, new List<Type> { consumerType });
                }
            }

            return queueConsumerMappings;
        }
    }
}
