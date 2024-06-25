using DotBoil.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DotBoil.MassTransit.Interceptors
{
    internal class MassTransitSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            var entries = eventData.Context.ChangeTracker.Entries();

            foreach (var entity in entries)
            {
                var baseEntity = entity.Entity as BaseEntity;

                if (baseEntity == null) continue;

                if (!baseEntity.DomainEvents.Any()) continue;

                foreach (var domainEvent in baseEntity.DomainEvents)
                {

                }
            }

            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
    }
}
