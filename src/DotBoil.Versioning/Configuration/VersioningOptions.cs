using DotBoil.Configuration;

namespace DotBoil.Versioning.Configuration
{
    internal class VersioningOptions : IOptions
    {
        public string Key => "DotBoil:Versioning";

        public int DefaultMajorVersion { get; set; }
        public int? DefaultMinorVersion { get; set; }
        public bool UrlSegmentApiVersioningEnable { get; set; }
        public bool HeaderApiVersioningEnable { get; set; }
        public string HeaderApiVersioningHeaderName { get; set; }
    }
}
