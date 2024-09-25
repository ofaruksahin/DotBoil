using Microsoft.EntityFrameworkCore;

namespace DotBoil.Localization.Persistence
{
    internal class LocalizationDbContext : DbContext
    {
        public DbSet<Models.Localization> Localizations { get; set; }

        public LocalizationDbContext(DbContextOptions<LocalizationDbContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Models.Localization>()
                .HasKey(p => p.Id);

            modelBuilder
                .Entity<Models.Localization>()
                .Property(p => p.Language)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder
                .Entity<Models.Localization>()
                .Property(p => p.Group)
                .HasMaxLength(100);

            modelBuilder
                .Entity<Models.Localization>()
                .Property(p => p.Key)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder
                .Entity<Models.Localization>()
                .Property(p => p.Value)
                .IsRequired();

            modelBuilder
                .Entity<Models.Localization>()
                .ToTable("Localizations");

            base.OnModelCreating(modelBuilder);
        }
    }
}
