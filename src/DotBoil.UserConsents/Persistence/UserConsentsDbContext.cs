using DotBoil.UserConsents.Models;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.UserConsents.Persistence
{
    internal class UserConsentsDbContext : DbContext
    {
        public DbSet<Consent> Consents { get; set; }
        public DbSet<ConsentHistory> ConsentHistories { get; set; }
        public DbSet<UserConsent> UserConsents { get; set; }

        public UserConsentsDbContext(DbContextOptions<UserConsentsDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Consent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Language).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.Version).IsRequired();
                entity.HasIndex(e => new { e.Type, e.Language }).IsUnique();
                entity.ToTable("Consents");
            });

            modelBuilder.Entity<ConsentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Language).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.IsRequired).IsRequired();
                entity.Property(e => e.Version).IsRequired();
                entity.ToTable("ConsentHistories");
            });

            modelBuilder.Entity<UserConsent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Language).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Version).IsRequired();
                entity.ToTable("UserConsents");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
