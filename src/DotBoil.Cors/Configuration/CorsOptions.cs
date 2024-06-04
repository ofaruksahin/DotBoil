using DotBoil.Configuration;

namespace DotBoil.Cors.Configuration
{
    internal class CorsOptions : IOptions
    {
        public string Key => "DotBoil:Cors";

        public string PolicyName { get; set; }
        public List<string> Origins { get; set; }
    }
}
