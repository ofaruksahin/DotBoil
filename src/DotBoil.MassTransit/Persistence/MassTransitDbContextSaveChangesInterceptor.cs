using DotBoil.Entities;
using DotBoil.MassTransit.Entities;
using DotBoil.MessageBroker;
using DotBoil.Serialization;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotBoil.MassTransit.Persistence
{
    internal class MassTransitDbContextSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IBusPublisher _busPublisher;
        private readonly MassTransitDbContext _context;

        public MassTransitDbContextSaveChangesInterceptor(
            IBusPublisher busPublisher,
            MassTransitDbContext context)
        {
            _busPublisher = busPublisher;
            _context = context;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            var entities = eventData
                .Context
                .ChangeTracker
                .Entries();

            var events = new List<IEvent>();

            foreach (var entry in entities)
            {
                var entity = entry.Entity;

                if (entity is BaseEntity baseEntity)
                {
                    foreach (var @event in baseEntity.Events)
                    {
                        events.Add(@event);

                        var outboxMessage = new OutboxMessage
                        {
                            Id = Guid.NewGuid(),
                            MessageType = @event.GetType().AssemblyQualifiedName,
                            Content = await (@event as object).SerializeAsync(),
                            CreateTime = DateTimeOffset.UtcNow,
                            Processed = false
                        };

                        await _context.Outbox.AddAsync(outboxMessage);

                        try
                        {
                            await _busPublisher.Publish(@event);
                            outboxMessage.Processed = true;
                        }
                        catch (Exception)
                        {
                        }                       
                    }
                }
            }

            if (events.Any())
                await _context.SaveChangesAsync();

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
