using DotBoil.Configuration;

namespace DotBoil.Studio.Core.Configurations;

public class PersistenceConfiguration : IOptions
{
    public string Key => "DotBoil:Studio:Persistence";

    public string ConnectionString { get; set; }
}