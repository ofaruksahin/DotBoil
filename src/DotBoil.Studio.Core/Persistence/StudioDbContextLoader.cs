using DotBoil.EFCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.Studio.Core.Persistence;

public class StudioDbContextLoader : EFCoreDbContextLoader
{
    public override Task LoadDbContext(IConfiguration configuration, IServiceCollection services)
    {
        services.AddDbContext<StudioDbContext>();
        return Task.CompletedTask;
    }
}