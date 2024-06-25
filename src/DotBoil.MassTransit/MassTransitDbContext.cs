using DotBoil.MassTransit.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.MassTransit
{
    public class MassTransitDbContext : DbContext
    {
        public DbSet<OutboxMessage> Outbox { get; set; }
        public DbSet<InboxMessage> Inbox { get; set; }

        public MassTransitDbContext(DbContextOptions<MassTransitDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<OutboxMessage>()
                .HasKey(p => p.Id);

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.Id)
                .IsRequired();

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.MessageType)
                .HasMaxLength(1024)
                .IsRequired();

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.QueueName)
                .HasMaxLength(512)
                .IsRequired();

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.Content)
                .HasMaxLength(8192)
                .IsRequired();

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.CreatedAt)
                .IsRequired();

            modelBuilder
                .Entity<OutboxMessage>()
                .Property(p => p.Processed)
                .IsRequired();

            modelBuilder
                .Entity<InboxMessage>()
                .HasKey(p => p.Id);

            modelBuilder
                .Entity<InboxMessage>()
                .Property(p => p.Id)
                .IsRequired();

            modelBuilder
                .Entity<InboxMessage>()
                .Property(p => p.MessageId)
                .IsRequired();

            modelBuilder
                .Entity<InboxMessage>()
                .Property(p => p.ProcessedAt)
                .IsRequired();
        }
    }
}
