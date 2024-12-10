using DotBoil.Configuration;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.ContextOptions;

public class DotBoilauthGuardDbContextOptions : IOptions
{
    public string Key => "DotBoil:AuthGuard:DbContext";
    public string ConnectionString { get; set; }
}