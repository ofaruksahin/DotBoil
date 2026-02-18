using DotBoil.Configuration;
using DotBoil.Cronos.Configuration;
using DotBoil.Cronos.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotBoil.Cronos.Persistence;

internal class CronosDbContext : DbContext
{
    public DbSet<ScheduledJobs> ScheduledJobs { get; set; }
    public DbSet<JobExecutions> JobExecutions { get; set; }

    public CronosDbContext()
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = DotBoilApp.Configuration.GetConfigurations<CronosConfiguration>();
        optionsBuilder.UseMySQL(configuration.ConnectionString);

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ScheduledJobs>()
            .HasKey(j => j.Id);

        modelBuilder
            .Entity<ScheduledJobs>()
            .Property(j => j.Name)
            .HasMaxLength(256)
            .IsRequired();

        modelBuilder
            .Entity<ScheduledJobs>()
            .Property(j => j.TypeName)
            .HasMaxLength(512)
            .IsRequired();

        modelBuilder
            .Entity<ScheduledJobs>()
            .Property(j => j.CronExpression)
            .HasMaxLength(128)
            .IsRequired();

        modelBuilder
            .Entity<ScheduledJobs>()
            .Property(j => j.TimeZoneId)
            .HasMaxLength(128)
            .IsRequired();

        modelBuilder
            .Entity<JobExecutions>()
            .HasKey(e => e.Id);

        modelBuilder
            .Entity<JobExecutions>()
            .HasOne(e => e.ScheduledJob)
            .WithMany(j => j.JobExecutions)
            .HasForeignKey(e => e.ScheduledJobId);

        base.OnModelCreating(modelBuilder);
    }
}
