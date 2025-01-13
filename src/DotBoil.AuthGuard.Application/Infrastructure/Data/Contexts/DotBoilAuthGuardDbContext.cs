using DotBoil.AuthGuard.Application.Domain.Entities;
using DotBoil.AuthGuard.Application.Infrastructure.Data.ContextOptions;
using DotBoil.Configuration;
using DotBoil.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;

public class DotBoilAuthGuardDbContext : EFCoreDbContext
{
    public DbSet<ApiEndpoint> ApiEndpoints { get; set; }
    public DbSet<AppModule> AppModules { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    
    public DotBoilAuthGuardDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    protected override void ConfigureDatabaseProvider(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        using var scope = _serviceProvider.CreateScope();
        var dbContextOptions = scope.ServiceProvider.GetService<DotBoilAuthGuardDbContextOptions>();
        
        optionsBuilder.UseMySQL(dbContextOptions.ConnectionString);
    }
}