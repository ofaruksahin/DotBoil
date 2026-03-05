using DotBoil.Configuration;

namespace DotBoil.Studio.Core.Configurations;

internal class ApiDataSourceConfiguration : IOptions
{
    public string Key => "DotBoil:DataSource:Api";

    public List<ApiDataSourceOptions> Sources { get; set; }

    public ApiDataSourceConfiguration()
    {
        Sources = new List<ApiDataSourceOptions>();
    }
    
    public class ApiDataSourceOptions
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string BaseUrl { get; set; }
    }
}