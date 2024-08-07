using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DotBoil.EFCore
{
    public abstract class EFCoreDbContextLoader
    {
        public abstract Task LoadDbContext(IConfiguration configuration, IServiceCollection services);
    }
}
