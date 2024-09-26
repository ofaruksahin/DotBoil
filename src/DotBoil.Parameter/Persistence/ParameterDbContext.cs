using DotBoil.Configuration;
using DotBoil.Parameter.Configurations;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Parameter.Persistence
{
    internal class ParameterDbContext : DbContext
    {
        public DbSet<Models.Parameter> Parameters { get; set; }

        public ParameterDbContext()
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = DotBoilApp.Configuration.GetConfigurations<ParameterConfiguration>();
            optionsBuilder.UseMySQL(configuration.Persistence.ConnectionString);

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Models.Parameter>()
                .HasKey(p => p.Id);

            modelBuilder
                .Entity<Models.Parameter>()
                .Property(p => p.Section)
                .HasMaxLength(128)
                .IsRequired();

            modelBuilder
                .Entity<Models.Parameter>()
                .Property(p => p.Key)
                .HasMaxLength(2048)
                .IsRequired();

            modelBuilder
                .Entity<Models.Parameter>()
                .Property(p => p.Value)
                .IsRequired();


            modelBuilder
                .Entity<Models.Parameter>()
                .ToTable("Parameters");

            base.OnModelCreating(modelBuilder);
        }
    }
}
