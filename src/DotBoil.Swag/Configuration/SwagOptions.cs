using DotBoil.Configuration;

namespace DotBoil.Swag.Configuration
{
    internal class SwagOptions : IOptions
    {
        public string Key => "DotBoil:Swagger";

        public string XmlFile { get; set; }
        public SwagContactOptions Contact { get; set; }
        public List<SwagVersionOptions> Versions { get; set; }

        public SwagOptions()
        {
            Versions = new List<SwagVersionOptions>();
        }
    }
}
