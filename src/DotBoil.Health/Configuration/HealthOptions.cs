using DotBoil.Configuration;
using DotBoil.Health.Configuration.UI;

namespace DotBoil.Health.Configuration
{
    internal class HealthOptions : IOptions
    {
        public string Key => "DotBoil:Health";

        public string Url { get; set; }
        public HealthUIOptions UI { get; set; }
    }
}
