using DotBoil.Configuration;

namespace DotBoil.Cronos.Configuration;

internal class CronosConfiguration : IOptions
{
    public string Key => "DotBoil:Cronos";

    public string ConnectionString { get; set; }
    public string RedisConnectionString { get; set; }
    public List<CronosJobConfiguration> Jobs { get; set; } = new();
}

internal class CronosJobConfiguration
{
    public string Name { get; set; }
    public string TypeName { get; set; }
    public string CronExpression { get; set; }
    public string TimeZoneId { get; set; }
    public bool IsActive { get; set; }
}
