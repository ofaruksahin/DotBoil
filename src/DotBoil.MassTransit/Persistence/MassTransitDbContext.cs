using DotBoil.MassTransit.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.MassTransit.Persistence
{
    internal class MassTransitDbContext : DbContext
    {
        public DbSet<OutboxMessage> Outbox { get; set; }
        public DbSet<InboxMessage> Inbox { get; set; }
        public DbSet<RetryPolicyException> RetryPolicyExceptions { get; set; }

        public MassTransitDbContext(DbContextOptions<MassTransitDbContext> options) : base(options)
        {

        }
    }
}
