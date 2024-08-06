using DotBoil.Configuration;

namespace DotBoil.TemplateEngine.Configuration
{
    internal class RazorViewEngineConfiguration : IOptions
    {
        public string Key => "DotBoil:ViewEngine:Razor";
        public string AssemblyName { get; set; }
        public string RootNamespace { get; set; }
    }
}
