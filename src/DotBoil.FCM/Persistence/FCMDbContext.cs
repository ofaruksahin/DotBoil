using DotBoil.FCM.Models;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.FCM.Persistence
{
    internal class FCMDbContext : DbContext
    {
        public DbSet<UserDeviceToken> UserDeviceTokens { get; set; }

        public FCMDbContext(DbContextOptions<FCMDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDeviceToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).HasMaxLength(UserDeviceToken.UserIdMaxLength).IsRequired();
                entity.Property(e => e.Token).HasMaxLength(UserDeviceToken.TokenMaxLength).IsRequired();
                entity.Property(e => e.Platform).IsRequired();
                entity.Property(e => e.DeviceId).HasMaxLength(UserDeviceToken.DeviceIdMaxLength);
                entity.Property(e => e.LastSeenTime).IsRequired();
                entity.HasIndex(e => e.Token).IsUnique();
                entity.HasIndex(e => new { e.UserId, e.IsDeleted });
                entity.ToTable("UserDeviceTokens");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
