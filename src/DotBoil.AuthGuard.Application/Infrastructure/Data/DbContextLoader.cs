using DotBoil.AuthGuard.Application.Infrastructure.Data.Contexts;
using DotBoil.EFCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data;

public class DbContextLoader : EFCoreDbContextLoader
{
    public override Task LoadDbContext(IConfiguration configuration, IServiceCollection services)
    {
        services.AddDbContext<DotBoilAuthGuardDbContext>();
        return Task.CompletedTask;
    }
}