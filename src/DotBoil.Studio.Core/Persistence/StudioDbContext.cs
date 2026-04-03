using DotBoil.EFCore;
using DotBoil.Studio.Core.Configurations;
using DotBoil.Studio.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Studio.Core.Persistence;

public class StudioDbContext : EFCoreDbContext
{
    public DbSet<UIConfig> UIConfigs { get; set; }
    public DbSet<UIConfigVersion> UIConfigVersions { get; set; }

    public StudioDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override void ConfigureDatabaseProvider(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        using var scope = _serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<PersistenceConfiguration>();
        optionsBuilder.UseMySQL(configuration.ConnectionString);
    }
}