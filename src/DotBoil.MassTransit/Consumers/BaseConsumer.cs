using DotBoil.MassTransit.Configuration;
using DotBoil.MassTransit.Entities;
using DotBoil.MassTransit.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.MassTransit.Consumers
{
    public abstract class BaseConsumer<TEvent> : IConsumer<TEvent>
        where TEvent : class, MessageBroker.IEvent
    {
        private readonly IServiceProvider _serviceProvider;

        protected BaseConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public abstract Task ConsumeEvent(ConsumeContext<TEvent> context);

        public async Task Consume(ConsumeContext<TEvent> context)
        {
            try
            {
                await ConsumeEvent(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
                throw;
            }
        }

        private async Task HandleException(ConsumeContext<TEvent> context, Exception ex)
        {
            using var scope = _serviceProvider.CreateScope();
            var rabbitMqConfiguration = _serviceProvider.GetService<MassTransitRabbitMqConfiguration>();
            var queueName = context.ReceiveContext.InputAddress.AbsolutePath.Trim('/');

            var massTransitDbContext = scope.ServiceProvider.GetService<MassTransitDbContext>();
            await massTransitDbContext.RetryPolicyExceptions.AddAsync(new RetryPolicyException(context.MessageId.Value, ex.Message));
            await massTransitDbContext.SaveChangesAsync();
        }
    }
}
