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

                using var scope = _serviceProvider.CreateScope();
                var massTransitDbContext = scope.ServiceProvider.GetRequiredService<MassTransitDbContext>();

                var inbox = new InboxMessage
                {
                    Id = Guid.NewGuid(),
                    MessageId = context.MessageId ?? Guid.Empty,
                    ProcessedTime = DateTime.UtcNow,
                };

                await massTransitDbContext.Inbox.AddAsync(inbox);
                await massTransitDbContext.SaveChangesAsync();
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

            var massTransitDbContext = scope.ServiceProvider.GetRequiredService<MassTransitDbContext>();
            var messageId = context.MessageId ?? Guid.Empty;

            await massTransitDbContext.RetryPolicyExceptions.AddAsync(
                new RetryPolicyException(messageId, ex.Message));
            await massTransitDbContext.SaveChangesAsync();
        }
    }
}
