using DotBoil.Configuration;

namespace DotBoil.AuthGuard.Application.Infrastructure.Data.ContextOptions;

public class DotBoilAuthGuardDbContextOptions : IOptions
{
    public string Key => "DotBoil:AuthGuard:DbContext";
    public string ConnectionString { get; set; }
}